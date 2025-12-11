using UnityEngine;

[CreateAssetMenu(menuName = "UI/Theme Data")]
public class UIThemeData : ScriptableObject
{
    [Header("Background Colors")]
    public Color lightBackground = Color.white;
    public Color darkBackground = new Color(0.1f, 0.1f, 0.1f);

    [Header("Text Colors")]
    public Color lightText = Color.black;
    public Color darkText = Color.white;

    [Header("Button Colors")]
    public Color lightButton = new Color(0.9f, 0.9f, 0.9f);
    public Color darkButton = new Color(0.2f, 0.2f, 0.2f);
}
