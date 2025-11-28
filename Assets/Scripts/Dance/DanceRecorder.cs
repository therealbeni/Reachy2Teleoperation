using Reachy.Part.Arm;
using Reachy.Part.Head;
using System;
using System.Collections.Generic;
using System.IO;
using TeleopReachy;
using UnityEngine;
using Newtonsoft.Json;  

public class DanceRecorder : MonoBehaviour
{
    [Header("Recording")]
    public bool allowRecording = true;

    [Header("Saving")]
    [Tooltip("Relative folder under Assets where recordings will be saved.")]
    public string recordingsFolderName = "dance_recordings";
    public string fileNamePrefix = "dance_";
    public string fileExtension = ".json";

    [Header("UI")]
    [SerializeField]
    private GameObject recorderIndicator;

    private UserMovementsInput userMovementsInput;
    private bool isRecording = false;
    private float recordingStartTime = 0.0f;

    // Now we store the DTOs defined in DanceSerializer
    private readonly List<DanceSerializer.PoseSampleData> recordedSamples =
        new List<DanceSerializer.PoseSampleData>();

    private void InitUserInputs()
    {
        userMovementsInput = UserInputManager.Instance != null
            ? UserInputManager.Instance.UserMovementsInput
            : null;

        if (userMovementsInput == null)
        {
            Debug.LogWarning("DanceRecorder: UserMovementsInput is null after InitUserInputs.");
        }
        else
        {
            Debug.Log("DanceRecorder: UserMovementsInput initialized.");
        }
    }

    private void Start()
    {
        EventManager.StartListening(EventNames.MirrorSceneLoaded, InitUserInputs);

        if (userMovementsInput == null && UserInputManager.Instance != null)
        {
            InitUserInputs();
        }

        if (recorderIndicator != null)
        {
            recorderIndicator.SetActive(false);
            Debug.Log("DanceRecorder: recorderIndicator found and set inactive on Start.");
        }
        else
        {
            Debug.LogWarning("DanceRecorder: recorderIndicator is not assigned in the Inspector.");
        }
    }

    private void OnDestroy()
    {
        EventManager.StopListening(EventNames.MirrorSceneLoaded, InitUserInputs);
    }

    private void BeginRecording()
    {
        if (!allowRecording)
        {
            Debug.LogWarning("DanceRecorder: Recording not allowed (allowRecording is false).");
            return;
        }

        if (userMovementsInput == null)
        {
            InitUserInputs();
            if (userMovementsInput == null)
            {
                Debug.LogWarning("DanceRecorder: Cannot start recording, userMovementsInput is null.");
                return;
            }
        }

        recordedSamples.Clear();
        recordingStartTime = Time.time;
        isRecording = true;

        if (recorderIndicator != null)
        {
            recorderIndicator.SetActive(true);
            Debug.Log($"DanceRecorder: recorderIndicator.SetActive(true). activeSelf={recorderIndicator.activeSelf}");
        }

        Debug.Log("DanceRecorder: Started Recording Dance");
    }

    private void StopRecording()
    {
        if (!isRecording)
            return;

        isRecording = false;

        if (recorderIndicator != null)
        {
            recorderIndicator.SetActive(false);
            Debug.Log($"DanceRecorder: recorderIndicator.SetActive(false). activeSelf={recorderIndicator.activeSelf}");
        }

        Debug.Log($"DanceRecorder: Stopped Recording Dance. Recorded {recordedSamples.Count} samples.");

        SaveRecordedDanceToFile();
    }

    private void ReadAndSaveCurrentPose()
    {
        if (userMovementsInput == null)
        {
            Debug.LogWarning("DanceRecorder: userMovementsInput is null, cannot read pose.");
            return;
        }

        // Get live teleop targets
        NeckJointGoal headTarget = userMovementsInput.GetHeadTarget();
        ArmCartesianGoal leftEndEffector = userMovementsInput.GetLeftEndEffectorTarget();
        ArmCartesianGoal rightEndEffector = userMovementsInput.GetRightEndEffectorTarget();

        float timestamp = Time.time - recordingStartTime;

        // Convert to DTO using DanceSerializer
        var sample = DanceSerializer.CreatePoseSample(
            headTarget,
            leftEndEffector,
            rightEndEffector,
            timestamp
        );

        recordedSamples.Add(sample);
    }

    private void SaveRecordedDanceToFile()
    {
        if (recordedSamples.Count == 0)
        {
            Debug.LogWarning("DanceRecorder: No samples recorded, skipping save.");
            return;
        }

        // Wrap into DanceSerializer.DanceRecordingData and save via DanceSerializer
        var data = new DanceSerializer.DanceRecordingData
        {
            samples = new List<DanceSerializer.PoseSampleData>(recordedSamples)
        };

        string folderPath = Path.Combine(Application.dataPath, recordingsFolderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{fileNamePrefix}{timestamp}{fileExtension}";
        string filePath = Path.Combine(folderPath, fileName);

        try
        {
            DanceSerializer.SaveToFile(data, filePath);
            Debug.Log($"DanceRecorder: Saved dance recording to: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"DanceRecorder: Failed to save recording to file. Exception: {e}");
        }
    }

    private void Update()
    {
        if (!allowRecording)
            return;

        // Editor keyboard test (optional)
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("DanceRecorder: Editor key R pressed -> BeginRecording()");
            BeginRecording();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("DanceRecorder: Editor key T pressed -> StopRecording()");
            StopRecording();
        }
#endif

        // Start recording (B)
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            Debug.Log("DanceRecorder: OVR B pressed -> BeginRecording()");
            BeginRecording();
        }

        // Stop recording and save (A)
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            Debug.Log("DanceRecorder: OVR A pressed -> StopRecording()");
            StopRecording();
        }

        // Record poses each frame
        if (isRecording)
        {
            ReadAndSaveCurrentPose();
        }
    }
}
