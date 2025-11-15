using UnityEngine;
using TeleopReachy;

public class MenuUI : MonoBehaviour
{
    public void StartModeDanceAfterReachy()
    {
        EventManager.TriggerEvent(EventNames.EnterSafetyDanceAfterReachyScene);
    }

    public void StartModeTabletop()
    {
        EventManager.TriggerEvent(EventNames.EnterSafetyTabletopScene);
    }

    public void StartModeDanceTeleop()
    {
        EventManager.TriggerEvent(EventNames.EnterSafetyDanceWithReachyScene);
    }

    // NEW BUTTON:
    public void BackToConnection()
    {
        EventManager.TriggerEvent(EventNames.EnterConnectionScene);
    }

    public void QuitApp()
    {
        EventManager.TriggerEvent(EventNames.QuitApplication);
    }
}