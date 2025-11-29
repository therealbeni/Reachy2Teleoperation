using UnityEngine;

public class RainbowGlow : MonoBehaviour
{
    public Renderer targetRenderer;
    public float speed = 1f;
    public float intensity = 3f;

    private Material mat;

    void Start()
    {
        mat = targetRenderer.material;
    }

    void Update()
    {
        // Cycle hue over time (0–1)
        float hue = Mathf.Repeat(Time.time * speed, 1f);

        // Convert HSV to RGB
        Color color = Color.HSVToRGB(hue, 1f, 1f);

        // Apply to emission color
        mat.SetColor("_EmissionColor", color * intensity);
    }
}
