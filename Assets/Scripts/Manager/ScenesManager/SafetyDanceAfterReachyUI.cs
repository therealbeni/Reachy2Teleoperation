using UnityEngine;
using TeleopReachy;

public class SafetyUISceneDanceAfterReachy : MonoBehaviour
{
    public void ContinueToGameOrPlaceholder()
    {
        // This remains a placeholder, per your design.
        Debug.Log("Add scene for dance after reachy stuff");
        //EventManager.TriggerEvent(EventNames.EnterMirrorScene);
    }

    public void BackToMenu()
    {
        EventManager.TriggerEvent(EventNames.EnterMenuFromSafetyScene);
    }
}