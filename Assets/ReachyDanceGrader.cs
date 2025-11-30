using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReachyDanceGrader : MonoBehaviour
{
    [Header("Tracked joints (same order for teacher & student)")]
    [Tooltip("Order must be: L shoulder, L elbow, L wrist, R shoulder, R elbow, R wrist.")]
    public Transform[] teacherJoints;

    [Tooltip("Corresponding joints of the robot on you (DanceReachy2) in the SAME order.")]
    public Transform[] studentJoints;

    [Header("Joint weights (applied to both arms)")]
    public float shoulderWeight = 2f;
    public float elbowWeight = 3f;
    public float wristWeight = 1.5f;

    [Header("Error thresholds (degrees)")]
    [Tooltip("Below this shoulder error we don't penalize.")]
    public float shoulderThresholdDeg = 5f;

    [Tooltip("At or above this shoulder error the frame can go to 0.")]
    public float shoulderZeroDeg = 45f;

    [Tooltip("Below this elbow error we don't penalize.")]
    public float elbowThresholdDeg = 5f;

    [Tooltip("At or above this elbow error the frame can go to 0.")]
    public float elbowZeroDeg = 45f;

    [Tooltip("Below this wrist error we don't penalize.")]
    public float wristThresholdDeg = 8f;

    [Tooltip("At or above this wrist error the frame can go to 0.")]
    public float wristZeroDeg = 60f;

    [Header("Trim non-graded time (seconds)")]
    [Tooltip("Seconds at the start of grading to ignore when computing the final score.")]
    public float trimStartSeconds = 2f;

    [Tooltip("Seconds at the end of grading to ignore when computing the final score.")]
    public float trimEndSeconds = 2f;

    [Header("UI Output")]
    public TMP_Text scoreText;

    private bool isGrading = false;
    private float gradingStartTime;

    // Store every frame’s score and time, then trim start/end and average
    private readonly List<float> frameScores = new List<float>();
    private readonly List<float> frameTimes = new List<float>();

    public float AverageScore01 { get; private set; } = 0f;
    public float AverageScorePercent => AverageScore01 * 100f;

    private void Start()
    {
        if (teacherJoints.Length != studentJoints.Length)
        {
            Debug.LogError("ReachyDanceGrader: teacherJoints and studentJoints must have same length.");
            return;
        }

        if (teacherJoints.Length % 3 != 0)
        {
            Debug.LogWarning("ReachyDanceGrader: expected joint count to be multiple of 3 " +
                             "(shoulder, elbow, wrist per arm). Current: " + teacherJoints.Length);
        }

        Debug.Log($"ReachyDanceGrader: grading {teacherJoints.Length} joint pairs.");
    }

    private void Update()
    {
        if (isGrading)
        {
            EvaluateFrame();
        }
    }

    // Call from UI / other scripts
    public void StartGrading()
    {
        if (teacherJoints == null || studentJoints == null ||
            teacherJoints.Length == 0 || studentJoints.Length == 0 ||
            teacherJoints.Length != studentJoints.Length)
        {
            Debug.LogWarning("ReachyDanceGrader: Joints not set up correctly, cannot start grading.");
            return;
        }

        frameScores.Clear();
        frameTimes.Clear();
        AverageScore01 = 0f;

        gradingStartTime = Time.time;
        isGrading = true;

        if (scoreText != null)
            scoreText.text = "Grading...";

        Debug.Log("ReachyDanceGrader: Grading started.");
    }

    // Call from UI / other scripts
    public void StopGrading()
    {
        if (!isGrading)
            return;

        isGrading = false;

        ComputeFinalScore();

        string msg = $"Accuracy: {AverageScorePercent:F1}%";
        Debug.Log("ReachyDanceGrader: Grading stopped. " + msg);

        if (scoreText != null)
            scoreText.text = msg;
    }

    private void EvaluateFrame()
    {
        float t = Time.time - gradingStartTime;      // seconds since grading started

        float worstPenalty = 0f;                     // 0..1, 1 = worst possible for this frame

        for (int i = 0; i < teacherJoints.Length; i++)
        {
            Transform tJoint = teacherJoints[i];
            Transform sJoint = studentJoints[i];

            if (tJoint == null || sJoint == null)
                continue;

            float angleError = Quaternion.Angle(tJoint.localRotation, sJoint.localRotation);

            // Determine joint type based on index pattern: 0/3 = shoulder, 1/4 = elbow, 2/5 = wrist
            int idx = i % 3;
            float threshold, zeroDeg, weight;

            if (idx == 0)   // shoulder
            {
                threshold = shoulderThresholdDeg;
                zeroDeg = shoulderZeroDeg;
                weight = shoulderWeight;
            }
            else if (idx == 1) // elbow
            {
                threshold = elbowThresholdDeg;
                zeroDeg = elbowZeroDeg;
                weight = elbowWeight;
            }
            else              // wrist
            {
                threshold = wristThresholdDeg;
                zeroDeg = wristZeroDeg;
                weight = wristWeight;
            }

            // Ensure sensible values
            zeroDeg = Mathf.Max(zeroDeg, threshold + 0.01f);

            float penalty = 0f;

            if (angleError <= threshold)
            {
                penalty = 0f; // no penalty if within tolerance
            }
            else if (angleError >= zeroDeg)
            {
                // full penalty for this joint type, scaled by weight
                penalty = Mathf.Clamp01(weight);
            }
            else
            {
                // between threshold and zeroDeg: linearly increasing penalty
                float tNorm = (angleError - threshold) / (zeroDeg - threshold); // 0..1
                penalty = Mathf.Clamp01(tNorm * weight);
            }

            // Worst joint defines the frame penalty
            if (penalty > worstPenalty)
                worstPenalty = penalty;
        }

        // Frame score is 1 - worstPenalty, then converted to percentage later
        float frameScore01 = Mathf.Clamp01(1f - worstPenalty);

        frameScores.Add(frameScore01);
        frameTimes.Add(t);
    }

    private void ComputeFinalScore()
    {
        if (frameScores.Count == 0)
        {
            AverageScore01 = 0f;
            return;
        }

        float totalDuration = frameTimes[frameTimes.Count - 1];
        float startCut = Mathf.Clamp(trimStartSeconds, 0f, totalDuration);
        float endCut = Mathf.Clamp(trimEndSeconds, 0f, totalDuration - startCut);

        float startTime = startCut;
        float endTime = totalDuration - endCut;

        float sum = 0f;
        int count = 0;

        for (int i = 0; i < frameScores.Count; i++)
        {
            float t = frameTimes[i];
            if (t < startTime || t > endTime)
                continue;

            sum += frameScores[i];
            count++;
        }

        if (count == 0)
        {
            AverageScore01 = 0f;
        }
        else
        {
            AverageScore01 = sum / count;
        }
    }
}
