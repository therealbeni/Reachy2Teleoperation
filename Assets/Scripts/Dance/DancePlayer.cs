using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Reachy2Controller;
using TeleopReachy;

public class DancePlayer : MonoBehaviour
{
    public DanceSequence sequence;
    public Reachy2Controller.Reachy2Controller reachy;

    private RobotJointCommands jointCommands;
    private RobotStatus robotStatus;

    public bool autoPlay = true;
    public bool useRealRobot = false;

    // Motor names MUST match Reachy2Controller.motors entries.
    private readonly string[] jointNamesL = new string[]
    {
        "l_arm_shoulder_axis_1",
        "l_arm_shoulder_axis_2",
        "l_arm_elbow_axis_1",
        "l_arm_elbow_axis_2",
        "l_arm_wrist_pitch",
        "l_arm_wrist_roll",
        "l_arm_wrist_yaw"
    };

    private readonly string[] jointNamesR = new string[]
    {
        "r_arm_shoulder_axis_1",
        "r_arm_shoulder_axis_2",
        "r_arm_elbow_axis_1",
        "r_arm_elbow_axis_2",
        "r_arm_wrist_pitch",
        "r_arm_wrist_roll",
        "r_arm_wrist_yaw"
    };

    private readonly string[] jointNamesHead = new string[]
    {
        "head_neck_roll",
        "head_neck_pitch",
        "head_neck_yaw"
    };

    private void Awake()
    {
        if (reachy == null)
            reachy = FindObjectOfType<Reachy2Controller.Reachy2Controller>();

        var data = RobotDataManager.Instance;
        if (data != null)
        {
            jointCommands = data.RobotJointCommands;
            robotStatus = data.RobotStatus;
        }

        if (reachy == null)
            Debug.LogError("[DancePlayer] No Reachy2Controller found in scene.");
        else
            Debug.Log($"[DancePlayer] Found Reachy2Controller with {reachy.motors.Length} motors.");
    }

    private void Start()
    {
        Debug.Log("[DancePlayer] Initialized.");
        if (autoPlay)
            StartDance();
    }

    public void StartDance()
    {
        if (sequence == null || sequence.steps == null || sequence.steps.Length == 0)
        {
            Debug.LogWarning("[DancePlayer] No dance sequence defined.");
            return;
        }

        if (reachy == null)
        {
            Debug.LogError("[DancePlayer] Cannot start dance, Reachy2Controller missing.");
            return;
        }

        Debug.Log("[DancePlayer] Starting dance sequence.");
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        Debug.Log("[DancePlayer] Playing dance sequence.");

        foreach (var step in sequence.steps)
        {
            float duration = Mathf.Max(0.01f, step.duration);

            float[] lStart = GetCurrentArmAngles(jointNamesL);
            float[] rStart = GetCurrentArmAngles(jointNamesR);
            float[] headStart = GetCurrentArmAngles(jointNamesHead);

            float[] lTarget = step.leftArmJoints;
            float[] rTarget = step.rightArmJoints;
            float[] headTarget = step.headJoints;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;

                if (lTarget != null && lTarget.Length == jointNamesL.Length)
                {
                    float[] lInterp = new float[jointNamesL.Length];
                    for (int i = 0; i < jointNamesL.Length; i++)
                        lInterp[i] = Mathf.Lerp(lStart[i], lTarget[i], t);

                    SendJointsImmediate(jointNamesL, lInterp);
                }

                if (rTarget != null && rTarget.Length == jointNamesR.Length)
                {
                    float[] rInterp = new float[jointNamesR.Length];
                    for (int i = 0; i < jointNamesR.Length; i++)
                        rInterp[i] = Mathf.Lerp(rStart[i], rTarget[i], t);

                    SendJointsImmediate(jointNamesR, rInterp);
                }

                if (headTarget != null && headTarget.Length == jointNamesHead.Length)
                {
                    float[] headInterp = new float[jointNamesHead.Length];
                    for (int i = 0; i < jointNamesHead.Length; i++)
                        headInterp[i] = Mathf.Lerp(headStart[i], headTarget[i], t);

                    SendJointsImmediate(jointNamesHead, headInterp);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // snap to final targets
            if (lTarget != null && lTarget.Length == jointNamesL.Length)
                SendJointsImmediate(jointNamesL, lTarget);

            if (rTarget != null && rTarget.Length == jointNamesR.Length)
                SendJointsImmediate(jointNamesR, rTarget);

            if (headTarget != null && headTarget.Length == jointNamesHead.Length)
                SendJointsImmediate(jointNamesHead, headTarget);
        }
    }

    private float[] GetCurrentArmAngles(string[] motorNames)
    {
        float[] result = new float[motorNames.Length];

        for (int i = 0; i < motorNames.Length; i++)
        {
            string name = motorNames[i];

            var motor = reachy.motors.FirstOrDefault(m => m.name == name);
            result[i] = motor != null ? motor.presentPosition : 0f;
        }

        return result;
    }

    private void SendJointsImmediate(string[] names, float[] anglesDeg)
    {
        var cmd = new Dictionary<string, float>();
        for (int i = 0; i < names.Length; i++)
            cmd[names[i]] = anglesDeg[i];

      
        // Unity prefab (always safe)
        if (reachy != null)
            reachy.HandleCommand(cmd);

        // Real robot 
        if (useRealRobot)
        {
            if (jointCommands != null &&
            robotStatus != null &&
            !robotStatus.AreRobotMovementsSuspended())
            {
                jointCommands.SendJointGoalPositions(cmd);
            }
            else
            {
                Debug.LogWarning(
                    $"[DancePlayer] Cannot send joint commands. " +
                    $"jointCommands={(jointCommands != null)}, " +
                    $"robotStatus={(robotStatus != null)}, " +
                    $"suspended={robotStatus?.AreRobotMovementsSuspended()}");
            }
        }
            
    }
}
