using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TeleopReachy;
using Reachy.Part.Head;
using Reachy.Part.Arm;
using Reachy2Controller;
using Newtonsoft.Json;
using Reachy.Part;   // used indirectly by DanceSerializer

public class DanceManager : MonoBehaviour
{
    [Header("Recording")]
    [Tooltip("Folder (relative to Application.dataPath) where dance JSON files are saved. Must match DanceRecorder.")]
    public string recordingsFolderName = "dance_recordings";

    [Tooltip("Name of the recording file, e.g. 'dance_20251125_163343.json'.")]
    public string recordingFileName = "dance_not_here.json";

    [Header("Playback")]
    [Tooltip("Play the recording in a loop.")]
    public bool loop = false;

    [Tooltip("Speed multiplier (1 = real time)")]
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
    public DanceSimulationServer simulatedServer;

    //getting grader to stop
    public ReachyDanceGrader grader;

    private enum PlaybackPhase
    {
        Idle,
        HomeStart,
        Playing,
        HomeEnd
    }

    private PlaybackPhase phase = PlaybackPhase.Idle;

    // Recording + samples (DTO)
    private DanceSerializer.DanceRecordingData recording;
    private DanceSerializer.PoseSampleData homePose;

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

    // Removed automatic playback from Start()

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

        string folderPath = Path.Combine(Application.streamingAssetsPath, recordingsFolderName);
        string filePath = Path.Combine(folderPath, recordingFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"DanceManager: Recording file not found: {filePath}");
            return false;
        }

        try
        {
            recording = DanceSerializer.LoadFromFile(filePath);

            if (recording == null || recording.samples == null || recording.samples.Count == 0)
            {
                Debug.LogError($"DanceManager: Loaded recording is empty: {filePath}");
                return false;
            }

            homePose = recording.samples[0]; // first sample = home pose
            Debug.Log($"DanceManager: Loaded recording: {filePath} ({recording.samples.Count} samples)");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"DanceManager: Failed to load recording. Exception: {e}");
            return false;
        }
    }

    // ---------- PUBLIC METHOD FOR BUTTON ----------
    public void OnStartButtonPressed()
    {
        Debug.Log("DanceManager: OnStartButtonPressed called.");
        StartPlayback();
    }
    // ----------------------------------------------

    public void StartPlayback()
    {
        if (recording == null || recording.samples.Count == 0)
        {
            Debug.LogWarning("DanceManager: No recording loaded, cannot start playback.");
            return;
        }

        if (!robotReady)
        {
            Debug.LogWarning("DanceManager: Robot not initialized, cannot start playback.");
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

        // Robot stiffen / control takeover
        EventManager.TriggerEvent(EventNames.OnRobotStiffRequested);
        EventManager.TriggerEvent(EventNames.OnStartDance);

        Debug.Log("DanceManager: Started playback.");
    }

    public void StopPlayback()
    {
        if (!isPlaying)
            return;

        isPlaying = false;
        phase = PlaybackPhase.Idle;

        grader.StopGrading();
        Debug.Log("DanceManager: Stopped playback.");

        EventManager.TriggerEvent(EventNames.OnStopDance);
        EventManager.TriggerEvent(EventNames.OnRobotSmoothlyCompliantRequested);
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

    private void SendPose(DanceSerializer.PoseSampleData pose)
    {
        if (pose == null)
            return;

        // Convert DTOs back to runtime goals
        NeckJointGoal headGoal = DanceSerializer.ToNeckJointGoal(pose.headTarget);
        ArmCartesianGoal leftGoal = DanceSerializer.ToArmCartesianGoal(pose.leftArm);
        ArmCartesianGoal rightGoal = DanceSerializer.ToArmCartesianGoal(pose.rightArm);

        if (leftGoal != null && leftGoal.Id == null)
            leftGoal.Id = new PartId { Name = "l_arm" };

        if (rightGoal != null && rightGoal.Id == null)
            rightGoal.Id = new PartId { Name = "r_arm" };

        // Real robot
        if (headGoal != null)
            jointsCommands.SendNeckCommands(headGoal);

        if (leftGoal != null || rightGoal != null)
            jointsCommands.SendArmsCommands(leftGoal, rightGoal);

        // Simulated robot
        if (simulatedServer != null)
        {
            if (headGoal != null) simulatedServer.SendNeckCommand(headGoal);
            if (leftGoal != null) simulatedServer.SendArmCommand(leftGoal);
            if (rightGoal != null) simulatedServer.SendArmCommand(rightGoal);
        }
    }
}
