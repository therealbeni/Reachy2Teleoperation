using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class AutoAssignEventCamera : MonoBehaviour
{
    void Awake()
    {
        var canvas = GetComponent<Canvas>();

        if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
        {
            // Use the main camera (OVR will usually tag the center eye as MainCamera)
            Camera cam = Camera.main;
            if (cam != null)
            {
                canvas.worldCamera = cam;
            }
            else
            {
                Debug.LogError("AutoAssignEventCamera: No main camera found for world-space canvas.");
            }
        }
    }
}
