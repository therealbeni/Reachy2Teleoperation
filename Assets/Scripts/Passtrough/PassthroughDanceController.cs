using Reachy.Kinematics;
using Reachy.Part.Arm;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeleopReachy
{
    [Serializable]
    public class ArmPoseMatrix
    {
        // 16 values, row-major, like RobotJointCommands' rArmZeroTarget/lArmZeroTarget
        public float[] data = new float[16];
    }

    [Serializable]
    public class DanceStep
    {
        public string name;
        [Min(0.1f)]
        public float duration = 1.0f;

        public ArmPoseMatrix leftArm = new ArmPoseMatrix();
        public ArmPoseMatrix rightArm = new ArmPoseMatrix();
    }

    public class PassthroughDanceController : MonoBehaviour
    {
        [Header("Dance definition")]
        public List<DanceStep> danceSteps = new List<DanceStep>();

        [Tooltip("Play automatically on Start")]
        public bool autoPlay = false;

        [Tooltip("Loop the whole dance when it finishes")]
        public bool loop = false;

        private RobotJointCommands jointCommands;
        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        private Coroutine playRoutine;

        private void Awake()
        {
            var dataMgr = RobotDataManager.Instance;
            jointCommands = dataMgr.RobotJointCommands;
            robotStatus = dataMgr.RobotStatus;
            robotConfig = dataMgr.RobotConfig;
        }

        private void Start()
        {
            // Optionally make the robot stiff the same way teleop does,
            // by triggering the same events teleop listens to.
            // This uses the same logic as RobotJointCommands.SetRobotStiff()
            EventManager.TriggerEvent(EventNames.OnRobotStiffRequested);

            if (autoPlay && danceSteps.Count > 0)
            {
                PlayDance();
            }
        }

        public void PlayDance()
        {
            if (danceSteps.Count == 0)
            {
                Debug.LogWarning("[PassthroughDanceController] No dance steps defined.");
                return;
            }

            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
            }

            playRoutine = StartCoroutine(PlayDanceCoroutine());
        }

        public void StopDance()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            // If you want, you can make the robot smoothly compliant again:
            // EventManager.TriggerEvent(EventNames.OnRobotSmoothlyCompliantRequested);
        }

        private IEnumerator PlayDanceCoroutine()
        {
            do
            {
                foreach (var step in danceSteps)
                {
                    yield return PlayStep(step);
                }
            }
            while (loop);
        }

        private IEnumerator PlayStep(DanceStep step)
        {
            if (!robotConfig.GotReachyConfig())
            {
                Debug.LogWarning("[PassthroughDanceController] Robot config not ready yet.");
                yield break;
            }

            // Build the two ArmCartesianGoal messages
            ArmCartesianGoal leftGoal = BuildArmGoal(step.leftArm);
            ArmCartesianGoal rightGoal = BuildArmGoal(step.rightArm);

            // Send once at the beginning, then you can optionally keep re-sending
            // during the duration to "hold" the pose.
            jointCommands.SendArmsCommands(leftGoal, rightGoal);

            float elapsed = 0f;
            float duration = Mathf.Max(0.05f, step.duration);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Optional: resubmit to ensure the pose is maintained
                jointCommands.SendArmsCommands(leftGoal, rightGoal);

                yield return null;
            }
        }

        private ArmCartesianGoal BuildArmGoal(ArmPoseMatrix pose)
        {
            Reachy.Kinematics.Matrix4x4 m = new Reachy.Kinematics.Matrix4x4();

            // Make sure array has 16 elements
            if (pose.data == null || pose.data.Length != 16)
            {
                pose.data = new float[16];
            }

            // Convert float[] → double[]
            double[] doubleArray = new double[16];
            for (int i = 0; i < 16; i++)
            {
                doubleArray[i] = (double)pose.data[i];
            }

            // Add doubles instead of floats
            m.Data.AddRange(doubleArray);

            var goal = new ArmCartesianGoal
            {
                GoalPose = m,
                ContinuousMode = IKContinuousMode.Unfreeze
            };

            // DO NOT assign Id or ConstrainedMode here.
            // RobotCommands.SendArmsCommands() will handle that.

            return goal;
        }
    }
}
