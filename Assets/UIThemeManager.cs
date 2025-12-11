using UnityEngine;

public class UIThemeManager : MonoBehaviour
{
    public UIThemeData themeData;
    public bool startDark = false;

    public static UIThemeManager Instance { get; private set; }
    public bool IsDarkMode { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        IsDarkMode = startDark;
        ApplyTheme();
    }

    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (themeData == null)
        {
            Debug.LogWarning("ThemeData not assigned!");
            return;
        }

        foreach (var element in FindObjectsOfType<UIThemeElement>(true))
        {
            element.ApplyTheme();
        }
    }
}