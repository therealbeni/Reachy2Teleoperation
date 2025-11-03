using UnityEngine;

public class LoadScene : MonoBehaviour
{
    public void GoToConnection()
    {
        TeleopReachy.EventManager.TriggerEvent(TeleopReachy.EventNames.EnterConnectionScene);
    }

    public void GoToSafety()
    {
        TeleopReachy.EventManager.TriggerEvent(TeleopReachy.EventNames.EnterSafetyScene);
    }
}
