using UnityEngine;

namespace TeleopReachyXR
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasInit : MonoBehaviour
    {
        [Tooltip("Distance in front of the camera where the menu will spawn.")]
        public float distanceFromCamera = 2f;

        [Tooltip("If true, the canvas will be placed in front of the camera once on Start.")]
        public bool placeOnceInFrontOfCamera = true;

        public Camera eventCamera;

        private void Start()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            Camera cam = eventCamera != null ? eventCamera : Camera.main;
            if (cam != null)
            {
                canvas.worldCamera = cam;

                if (placeOnceInFrontOfCamera)
                {
                    // Position 2m in front of camera
                    Transform ct = cam.transform;
                    Vector3 forwardFlat = new Vector3(ct.forward.x, 0f, ct.forward.z).normalized;
                    if (forwardFlat.sqrMagnitude < 0.0001f)
                        forwardFlat = ct.forward;

                    transform.position = ct.position + forwardFlat * distanceFromCamera;
                    transform.rotation = Quaternion.LookRotation(forwardFlat, Vector3.up);
                }
            }
        }
    }
}
