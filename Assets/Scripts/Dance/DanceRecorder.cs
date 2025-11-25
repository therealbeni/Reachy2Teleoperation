using Reachy.Part.Arm;
using Reachy.Part.Head;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TeleopReachy;
using UnityEngine;

public class DanceRecorder : MonoBehaviour
{

    [Serializable]
    public class PoseSample
    {
        public NeckJointGoal headTarget;
        public ArmCartesianGoal leftEndEffector;
        public ArmCartesianGoal rightEndEffector;
        public float timestamp;
    }

    [Serializable]
    public class  DanceRecordingData
    {
        public List<PoseSample> samples = new List<PoseSample>();
    }

    public bool allowRecording = true;

    [Header("Saving")]
    public string recordingsFolderName = "Resources/DanceRecordings";
    public string fileNamePrefix = "dance_";
    public string fileExtension = ".json";

    private UserMovementsInput userMovementsInput;
    private bool isRecording = false;
    private float recordingStartTime = 0.0f;

    private readonly List<PoseSample> recordedSamples = new List<PoseSample>();
    public GameObject recorderIndicator;


    void InitUserInputs()
    {
        userMovementsInput = UserInputManager.Instance != null
            ? UserInputManager.Instance.UserMovementsInput
            : null;

        if (userMovementsInput == null)
        {
            Debug.LogWarning("DanceRecorder: UserMovementsInput is null after InitUserInputs.");
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        EventManager.StartListening(EventNames.MirrorSceneLoaded, InitUserInputs);

        if (userMovementsInput == null && UserInputManager.Instance != null)
        {
            InitUserInputs();
        }

        if (recorderIndicator != null)
            recorderIndicator.SetActive(false);
    }

    private void OnDestroy()
    {
        EventManager.StopListening(EventNames.MirrorSceneLoaded, InitUserInputs);
    }

    void BeginRecording()
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
        }

        Debug.Log("DanceRecorder: Started Recording Dance");
    }

    void StopRecording()
    {
        if (!isRecording)
            return;

        isRecording = false;

        if (recorderIndicator != null)
        {
            recorderIndicator.SetActive(false);
        }

        Debug.Log($"DanceRecorder: Stopped Recording Dance. Recorded {recordedSamples.Count} samples.");

        SaveRecordedDanceToFile();
    }

    void ReadAndSaveCurrentPose()
    {
        if (userMovementsInput == null)
        {
            Debug.LogWarning("DanceRecorder: userMovementsInput is null, cannot read pose.");
            return;
        }

        NeckJointGoal headTarget = userMovementsInput.GetHeadTarget();
        ArmCartesianGoal leftEndEffector = userMovementsInput.GetLeftEndEffectorTarget();
        ArmCartesianGoal rightEndEffector = userMovementsInput.GetRightEndEffectorTarget();

        PoseSample sample = new PoseSample
        {
            timestamp = Time.time - recordingStartTime,
            headTarget = headTarget,
            leftEndEffector = leftEndEffector,
            rightEndEffector = rightEndEffector
        };

        recordedSamples.Add(sample);
    }

    void SaveRecordedDanceToFile()
    {
        if (recordedSamples.Count == 0)
        {
            Debug.LogWarning("DanceRecorder: No samples recorded, skipping save.");
            return;
        }

        DanceRecordingData data = new DanceRecordingData
        {
            samples = new List<PoseSample>(recordedSamples)
        };

        string json = JsonUtility.ToJson(data, true);

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
            File.WriteAllText(filePath, json);
            Debug.Log($"DanceRecorder: Saved dance recording to: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"DanceRecorder: Failed to save recording to file. Exception: {e}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!allowRecording)
            return;

        // Start recording (B)
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            BeginRecording();
        }

        // Stop recording and save (A)
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            StopRecording();
        }

        // Record poses each frame
        if (isRecording)
        {
            ReadAndSaveCurrentPose();
        }
    }
}


