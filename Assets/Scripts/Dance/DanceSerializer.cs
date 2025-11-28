using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Reachy.Part.Head;
using Reachy.Part.Arm;
using Reachy.Kinematics;
using Google.Protobuf.Collections;   // <-- for RepeatedField<T>

namespace TeleopReachy
{
    /// <summary>
    /// Contains serializable data structures for dance recordings and
    /// helper methods to save/load them as JSON, plus conversion
    /// helpers to/from NeckJointGoal and ArmCartesianGoal so we can
    /// drive the robot like in teleoperation.
    /// </summary>
    public static class DanceSerializer
    {
        #region Serializable DTOs (what we store in JSON)

        [Serializable]
        public class SerializableQuaternion
        {
            public float w;
            public float x;
            public float y;
            public float z;
        }

        /// <summary>
        /// Serializable representation of the head target.
        /// For now we only store orientation (quaternion in robot space).
        /// </summary>
        [Serializable]
        public class SerializableHeadTarget
        {
            public SerializableQuaternion rotation;
        }

        /// <summary>
        /// Serializable representation of a 4x4 pose matrix for the arm.
        /// We store the 16 elements of Reachy.Kinematics.Matrix4x4.Data.
        /// </summary>
        [Serializable]
        public class SerializableMatrix4x4
        {
            public float[] data = new float[16];
        }

        /// <summary>
        /// Serializable representation of an arm end-effector goal.
        /// </summary>
        [Serializable]
        public class SerializableArmTarget
        {
            public SerializableMatrix4x4 goalPose;
        }

        /// <summary>
        /// One pose sample in the recording.
        /// </summary>
        [Serializable]
        public class PoseSampleData
        {
            public SerializableHeadTarget headTarget;
            public SerializableArmTarget leftArm;
            public SerializableArmTarget rightArm;
            public float timestamp;
        }

        /// <summary>
        /// Full recording containing all pose samples.
        /// </summary>
        [Serializable]
        public class DanceRecordingData
        {
            public List<PoseSampleData> samples = new List<PoseSampleData>();
        }

        #endregion

        #region Conversion: NeckJointGoal <-> SerializableHeadTarget

        public static SerializableHeadTarget ToSerializableHead(NeckJointGoal headGoal)
        {
            if (headGoal == null ||
                headGoal.JointsGoal == null ||
                headGoal.JointsGoal.Rotation == null ||
                headGoal.JointsGoal.Rotation.Q == null)
            {
                return null;
            }

            Quaternion q = headGoal.JointsGoal.Rotation.Q;

            return new SerializableHeadTarget
            {
                rotation = new SerializableQuaternion
                {
                    w = (float)q.W,
                    x = (float)q.X,
                    y = (float)q.Y,
                    z = (float)q.Z
                }
            };
        }

        public static NeckJointGoal ToNeckJointGoal(SerializableHeadTarget head)
        {
            if (head == null || head.rotation == null)
                return null;

            var q = head.rotation;

            return new NeckJointGoal
            {
                JointsGoal = new NeckOrientation
                {
                    Rotation = new Rotation3d
                    {
                        Q = new Quaternion
                        {
                            W = q.w,
                            X = q.x,
                            Y = q.y,
                            Z = q.z,
                        }
                    }
                }
            };
        }

        #endregion

        #region Conversion: ArmCartesianGoal <-> SerializableArmTarget

        /// <summary>
        /// Convert Reachy.Kinematics.Matrix4x4 to a flat serializable structure.
        /// Matrix4x4.Data is a RepeatedField<double>, so we use Count and cast.
        /// </summary>
        public static SerializableMatrix4x4 ToSerializableMatrix(Matrix4x4 m)
        {
            if (m == null || m.Data == null || m.Data.Count < 16)
                return null;

            var s = new SerializableMatrix4x4
            {
                data = new float[16]
            };

            for (int i = 0; i < 16; i++)
            {
                // explicit cast from double to float
                s.data[i] = (float)m.Data[i];
            }

            return s;
        }

        /// <summary>
        /// Convert from serialized matrix to Reachy.Kinematics.Matrix4x4.
        /// We create a new Matrix4x4 and fill its RepeatedField<double> with 16 entries.
        /// </summary>
        public static Matrix4x4 ToMatrix4x4(SerializableMatrix4x4 s)
        {
            if (s == null || s.data == null || s.data.Length < 16)
                return null;

            var m = new Matrix4x4();

            // RepeatedField<double> starts empty, so we Add 16 values.
            for (int i = 0; i < 16; i++)
            {
                m.Data.Add((double)s.data[i]);   // explicit float->double
            }

            return m;
        }

        public static SerializableArmTarget ToSerializableArm(ArmCartesianGoal armGoal)
        {
            if (armGoal == null || armGoal.GoalPose == null)
                return null;

            return new SerializableArmTarget
            {
                goalPose = ToSerializableMatrix(armGoal.GoalPose)
            };
        }

        public static ArmCartesianGoal ToArmCartesianGoal(SerializableArmTarget arm)
        {
            if (arm == null || arm.goalPose == null)
                return null;

            return new ArmCartesianGoal
            {
                GoalPose = ToMatrix4x4(arm.goalPose)
            };
        }

        #endregion

        #region Helpers to create PoseSampleData at record time

        public static PoseSampleData CreatePoseSample(
            NeckJointGoal headTarget,
            ArmCartesianGoal leftEndEffector,
            ArmCartesianGoal rightEndEffector,
            float timestamp)
        {
            return new PoseSampleData
            {
                headTarget = ToSerializableHead(headTarget),
                leftArm = ToSerializableArm(leftEndEffector),
                rightArm = ToSerializableArm(rightEndEffector),
                timestamp = timestamp
            };
        }

        #endregion

        #region JSON serialization helpers

        public static void SaveToFile(DanceRecordingData recording, string filePath)
        {
            if (recording == null)
                throw new ArgumentNullException(nameof(recording));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath is null or empty.", nameof(filePath));

            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonConvert.SerializeObject(recording, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static DanceRecordingData LoadFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath is null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Recording file not found.", filePath);

            string json = File.ReadAllText(filePath);
            var recording = JsonConvert.DeserializeObject<DanceRecordingData>(json);

            if (recording == null || recording.samples == null)
            {
                recording = new DanceRecordingData();
            }

            return recording;
        }

        #endregion
    }
}
