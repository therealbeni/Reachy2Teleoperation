using UnityEngine;

public class ReachyVisibilityToggle : MonoBehaviour
{
    [Header("Icons")]
    public GameObject showIcon;   // eye+robot, visible state
    public GameObject hideIcon;   // crossed eye+robot, hidden state

    [Header("Reachy Root Object")]
    public GameObject reachyRoot; // DancyReachy2

    private Renderer[] reachyRenderers;
    private bool isVisible = true;

    void Awake()
    {
        if (reachyRoot != null)
        {
            // All renderers, including children; object stays active in scene
            reachyRenderers = reachyRoot.GetComponentsInChildren<Renderer>(true);
        }

        SetState(true); // start visible, show "show" icon
    }

    public void Toggle()
    {
        isVisible = !isVisible;
        SetState(isVisible);
    }

    private void SetState(bool visible)
    {
        // Toggle icons
        if (showIcon != null) showIcon.SetActive(!visible);
        if (hideIcon != null) hideIcon.SetActive(visible);

        // Toggle only rendering, DO NOT disable the GameObject
        if (reachyRenderers != null)
        {
            foreach (var r in reachyRenderers)
            {
                if (r != null) r.enabled = visible;
            }
        }
    }
}
