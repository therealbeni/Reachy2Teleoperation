using UnityEngine;

[CreateAssetMenu(fileName = "NewDanceStep", menuName = "Reachy/Dance Step")]
public class DanceStep : ScriptableObject
{
    [Header("Name of this pose")]
    public string stepName;

    [Header("Duration in seconds")]
    [Min(0.1f)]
    public float duration = 1f;

    [Header("Right arm joint angles (degrees)")]
    [Tooltip("7 joint angles: shoulder_pitch, shoulder_roll, arm_yaw, elbow_pitch, wrist_pitch, wrist_roll, wrist_yaw")]
    public float[] rightArmJoints = new float[7];

    [Header("Left arm joint angles (degrees)")]
    public float[] leftArmJoints = new float[7];
}
