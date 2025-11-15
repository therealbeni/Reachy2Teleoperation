using UnityEngine;
using TeleopReachy;

public class SafetyUISceneDanceTeleop : MonoBehaviour
{
    public void ContinueToDanceMirror()
    {
        EventManager.TriggerEvent(EventNames.EnterDanceMirrorScene);
    }

    public void BackToMenu()
    {
        EventManager.TriggerEvent(EventNames.EnterMenuFromSafetyScene);
    }
}