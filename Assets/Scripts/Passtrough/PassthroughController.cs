using UnityEngine;

public class PassthroughController : MonoBehaviour
{
    [SerializeField] private OVRPassthroughLayer layer;

    [Header("Debug")]
    [SerializeField] private bool _isEnabled;   // shows in Inspector

    public bool IsEnabled => _isEnabled;        // read-only for other scripts

    private void Awake()
    {
        if (!layer)
            layer = GetComponent<OVRPassthroughLayer>();
    }

    public void SetPassthrough(bool enabled)
    {
        _isEnabled = enabled;

        if (layer)
            layer.enabled = enabled;

        if (OVRManager.instance)
            OVRManager.instance.isInsightPassthroughEnabled = enabled;

        Debug.Log($"[PassthroughController] Passthrough {(enabled ? "ENABLED" : "DISABLED")}");
    }
}
