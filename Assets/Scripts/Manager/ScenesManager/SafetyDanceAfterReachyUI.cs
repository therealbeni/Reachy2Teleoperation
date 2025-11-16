using UnityEngine;
using TeleopReachy;

public class SafetyUISceneDanceAfterReachy : MonoBehaviour
{
    public void ContinueToPasstrough()
    {
        EventManager.TriggerEvent(EventNames.EnterPasstroughFromSafetyScene);
    }

    public void BackToMenu()
    {
        EventManager.TriggerEvent(EventNames.EnterMenuFromSafetyScene);
    }
}