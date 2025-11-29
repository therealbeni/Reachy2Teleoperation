using UnityEngine;
using Unity.XR.CoreUtils;   // Needed for XROrigin

public class PlaceDanceFloor : MonoBehaviour
{
    private Transform playerCamera;

    void Start()
    {
        // Find XR Origin in the base scene
        var xrOrigin = FindObjectOfType<XROrigin>();

        if (xrOrigin == null)
        {
            Debug.LogError("PlaceDanceFloor: No XR Origin found in any loaded scene!");
            return;
        }

        // Get the camera from XR Origin
        playerCamera = xrOrigin.Camera.transform;

        // Real floor = y = 0 (due to Floor Level tracking)
        float floorY = 0f;

        // Position the floor under the player's starting X/Z
        transform.position = new Vector3(
            playerCamera.position.x,
            floorY,
            playerCamera.position.z
        );

        // Align rotation to player's facing direction
        transform.rotation = Quaternion.Euler(
            0,
            playerCamera.eulerAngles.y,
            0
        );
    }
}
