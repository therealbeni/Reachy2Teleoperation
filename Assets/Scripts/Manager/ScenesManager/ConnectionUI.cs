using UnityEngine;
using TeleopReachy;

public class ConnectionUI : MonoBehaviour
{
    // Hook this to the "Quit" button in ConnectionScene
    public void QuitApplication()
    {
        EventManager.TriggerEvent(EventNames.QuitApplication);
    }
}