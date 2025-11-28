using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wave : MonoBehaviour
{
    // IMPORTANT: fully-qualified class name!
    public Reachy2Controller.Reachy2Controller controller;

    private string shoulderPitch = "r_shoulder_pitch";
    private string elbowPitch = "r_elbow_pitch";
    private string wristRoll = "r_wrist_roll";

    void Start()
    {
        StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        // 1. lift arm
        controller.HandleCommand(new Dictionary<string, float>()
        {
            { shoulderPitch, -45f },
            { elbowPitch, 25f },
            { wristRoll, 0f }
        });

        yield return new WaitForSeconds(0.6f);

        // 2. wave
        for (int i = 0; i < 4; i++)
        {
            controller.HandleCommand(new Dictionary<string, float>()
            {
                { wristRoll, -35f }
            });
            yield return new WaitForSeconds(0.25f);

            controller.HandleCommand(new Dictionary<string, float>()
            {
                { wristRoll, 35f }
            });
            yield return new WaitForSeconds(0.25f);
        }

        // 3. reset
        controller.HandleCommand(new Dictionary<string, float>()
        {
            { shoulderPitch, 0f },
            { elbowPitch, 0f },
            { wristRoll, 0f }
        });
    }
}
