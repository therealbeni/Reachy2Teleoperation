using UnityEngine;

namespace TeleopReachyXR
{
    public class FloatingCanvasInit : MonoBehaviour
    {
        [SerializeField]
        private float PlaneDistance;

        void Start()
        {
            // Assigne la caméra de Basescene au canva courant
            transform.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            transform.GetComponent<Canvas>().worldCamera = Camera.main;

            if (PlaneDistance != 0) transform.GetComponent<Canvas>().planeDistance = PlaneDistance;
        }

    }
}