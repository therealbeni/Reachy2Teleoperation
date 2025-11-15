using TeleopReachy;
using UnityEngine;

public class LoadScene : MonoBehaviour
{
    public void GoToConnectionFromSafety()
    {
        TeleopReachy.EventManager.TriggerEvent(TeleopReachy.EventNames.EnterConnectionFromSafetyScene);
    }

    public void GoToSafety()
    {
        TeleopReachy.EventManager.TriggerEvent(TeleopReachy.EventNames.EnterSafetyScene);
    }

    public void GoToMenuFromConnection()
    {
        TeleopReachy.EventManager.TriggerEvent(TeleopReachy.EventNames.EnterMenuFromConnectionScene);
    }

    public void GoToMenuFromSafety()
    {
        TeleopReachy.EventManager.TriggerEvent(TeleopReachy.EventNames.EnterMenuFromSafetyScene);
    }

    public void QuitApplication()
    {
        EventManager.TriggerEvent(EventNames.QuitApplication);
    }
}