using UnityEngine;
using TeleopReachy;

public class SafetyUISceneTabletop : MonoBehaviour
{
    public void ContinueToTabletopMirror()
    {
        EventManager.TriggerEvent(EventNames.EnterTabletopMirrorScene);
    }

    public void BackToMenu()
    {
        EventManager.TriggerEvent(EventNames.EnterMenuFromSafetyScene);
    }
}