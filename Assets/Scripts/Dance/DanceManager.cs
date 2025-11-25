using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TeleopReachy;
using Reachy.Part.Head;
using Reachy.Part.Arm;
using Reachy2Controller;
using Newtonsoft.Json;   // make sure Newtonsoft.Json is available in the project

public class DanceManager : MonoBehaviour
{
    [Serializable]
    private class PoseSample
    {
        public NeckJointGoal headTarget;
        public ArmCartesianGoal leftEndEffector;
        public ArmCartesianGoal rightEndEffector;
        public float timestamp;
    }

    [Serializable]
    private class DanceRecordingData
    {
        public List<PoseSample> samples = new List<PoseSample>();
    }

    [Header("Recording")]
    [Tooltip("Folder (relative to Application.dataPath) where dance JSON files are saved. Must match DanceRecorder.")]
    public string recordingsFolderName = "dance_recordings";

    [Tooltip("Name of the recording file, e.g. 'dance_20251125_163343.json'.")]
    public string recordingFileName = "dance_20251125_163343.json";

    [Header("Playback")]
    [Tooltip("Automatically start playback once robot + recording are ready.")]
    public bool autoPlayOnStart = true;

    [Tooltip("Play the recording in a loop.")]
    public bool loop = false;

    [Tooltip("Speed multiplier (1 = real time, 2 = twice as fast, 0.5 = half speed).")]
    public float playbackSpeed = 1.0f;

    [Header("Home pose / safety")]
    [Tooltip("Use the first recorded pose as a 'home' pose at the beginning and end.")]
    public bool useHomePose = true;

    [Tooltip("How long to hold the home pose at the beginning (seconds).")]
    public float homePoseHoldAtStart = 1.0f;

    [Tooltip("How long to hold the home pose at the end (seconds).")]
    public float homePoseHoldAtEnd = 1.0f;

    [Header("Simulation (optional)")]
    [Tooltip("If assigned, the simulated Reachy will also be driven by the recording.")]
    public ReachySimulatedServer simulatedServer;

    private enum PlaybackPhase
    {
        Idle,
        HomeStart,
        Playing,
        HomeEnd
    }

    private PlaybackPhase phase = PlaybackPhase.Idle;

    private DanceRecordingData recording;
    private PoseSample homePose;
    private float phaseStartTime;
    private float playbackStartTime;
    private int currentSampleIndex;
    private bool isPlaying;

    // Robot references
    private RobotStatus robotStatus;
    private RobotJointCommands jointsCommands;
    private bool robotReady;

    private void Awake()
    {
        InitRobotRefs();
        LoadRecording();
    }

    private void Start()
    {
        if (autoPlayOnStart && robotReady && recording != null && recording.samples.Count > 0)
        {
            StartPlayback();
        }
    }

    private void InitRobotRefs()
    {
        if (RobotDataManager.Instance == null)
        {
            Debug.LogError("DanceManager: RobotDataManager.Instance is null. " +
                           "Make sure the RobotDataScene (or equivalent) is loaded.");
            robotReady = false;
            return;
        }

        robotStatus = RobotDataManager.Instance.RobotStatus;
        jointsCommands = RobotDataManager.Instance.RobotJointCommands;

        robotReady = (robotStatus != null && jointsCommands != null);

        if (!robotReady)
        {
            Debug.LogError("DanceManager: RobotStatus or RobotJointCommands is null.");
        }
    }

    private bool LoadRecording()
    {
        if (string.IsNullOrEmpty(recordingFileName))
        {
            Debug.LogError("DanceManager: recordingFileName is empty.");
            return false;
        }

        string folderPath = Path.Combine(Application.dataPath, recordingsFolderName);
        string filePath = Path.Combine(folderPath, recordingFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"DanceManager: Recording file not found: {filePath}");
            return false;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            recording = JsonConvert.DeserializeObject<DanceRecordingData>(json);

            if (recording == null || recording.samples == null || recording.samples.Count == 0)
            {
                Debug.LogError($"DanceManager: Loaded recording is empty: {filePath}");
                return false;
            }

            homePose = recording.samples[0]; // first sample used as home pose
            Debug.Log($"DanceManager: Loaded recording: {filePath} ({recording.samples.Count} samples)");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"DanceManager: Failed to load recording from {filePath}. Exception: {e}");
            return false;
        }
    }

    public void StartPlayback()
    {
        if (recording == null || recording.samples == null || recording.samples.Count == 0)
        {
            Debug.LogWarning("DanceManager: No recording loaded, cannot start playback.");
            return;
        }

        if (!robotReady)
        {
            Debug.LogWarning("DanceManager: Robot is not initialized, cannot start playback.");
            return;
        }

        currentSampleIndex = 0;
        isPlaying = true;

        if (useHomePose && homePose != null)
        {
            phase = PlaybackPhase.HomeStart;
            phaseStartTime = Time.time;
        }
        else
        {
            phase = PlaybackPhase.Playing;
            playbackStartTime = Time.time;
        }

        // Optional: stiffen robot and broadcast that something is controlling it
        EventManager.TriggerEvent(EventNames.OnRobotStiffRequested);
        EventManager.TriggerEvent(EventNames.OnStartTeleoperation);

        Debug.Log("DanceManager: Started playback.");
    }

    public void StopPlayback()
    {
        if (!isPlaying)
            return;

        isPlaying = false;
        phase = PlaybackPhase.Idle;
        Debug.Log("DanceManager: Stopped playback.");

        EventManager.TriggerEvent(EventNames.OnStopTeleoperation);
        EventManager.TriggerEvent(EventNames.OnRobotCompliantRequested);
    }

    private void Update()
    {
        if (!isPlaying || recording == null || !robotReady)
            return;

        if (robotStatus != null && robotStatus.AreRobotMovementsSuspended())
            return;

        switch (phase)
        {
            case PlaybackPhase.HomeStart:
                PlayHomeStartPhase();
                break;
            case PlaybackPhase.Playing:
                PlayMainPhase();
                break;
            case PlaybackPhase.HomeEnd:
                PlayHomeEndPhase();
                break;
            case PlaybackPhase.Idle:
            default:
                break;
        }
    }

    private void PlayHomeStartPhase()
    {
        SendPose(homePose);

        float elapsed = Time.time - phaseStartTime;
        if (elapsed >= homePoseHoldAtStart)
        {
            phase = PlaybackPhase.Playing;
            playbackStartTime = Time.time;
            currentSampleIndex = 0;
        }
    }

    private void PlayMainPhase()
    {
        float t = (Time.time - playbackStartTime) * Mathf.Max(playbackSpeed, 0.0001f);

        while (currentSampleIndex < recording.samples.Count &&
               recording.samples[currentSampleIndex].timestamp <= t)
        {
            SendPose(recording.samples[currentSampleIndex]);
            currentSampleIndex++;
        }

        if (currentSampleIndex >= recording.samples.Count)
        {
            if (useHomePose && homePose != null)
            {
                phase = PlaybackPhase.HomeEnd;
                phaseStartTime = Time.time;
            }
            else if (loop)
            {
                playbackStartTime = Time.time;
                currentSampleIndex = 0;
            }
            else
            {
                StopPlayback();
            }
        }
    }

    private void PlayHomeEndPhase()
    {
        SendPose(homePose);

        float elapsed = Time.time - phaseStartTime;
        if (elapsed >= homePoseHoldAtEnd)
        {
            if (loop)
            {
                phase = PlaybackPhase.HomeStart;
                phaseStartTime = Time.time;
            }
            else
            {
                StopPlayback();
            }
        }
    }

    private void SendPose(PoseSample pose)
    {
        if (pose == null)
            return;

        // Real robot
        jointsCommands.SendNeckCommands(pose.headTarget);
        jointsCommands.SendArmsCommands(pose.leftEndEffector, pose.rightEndEffector);

        Debug.Log("DanceManager: Sent pose at timestamp " + pose.timestamp);

        // Simulated robot (optional)
        if (simulatedServer != null)
        {
            if (pose.headTarget != null)
                Debug.Log("DanceManager: Sending head command to simulated server.");
                simulatedServer.SendNeckCommand(pose.headTarget);

            if (pose.leftEndEffector != null)
                simulatedServer.SendArmCommand(pose.leftEndEffector);

            if (pose.rightEndEffector != null)
                simulatedServer.SendArmCommand(pose.rightEndEffector);
        }
    }
}
