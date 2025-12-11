using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIAutoTheme : MonoBehaviour
{
    [Header("Optional Theme Toggle Button")]
    public Button themeToggleButton;

    private void Awake()
    {
        // Apply theme immediately on scene load
        if (UIThemeManager.Instance != null)
        {
            UIThemeManager.Instance.ApplyTheme();
        }

        // Listen for scene changes → auto update when changing scenes
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Hook up the toggle button (if assigned)
        if (themeToggleButton != null)
        {
            themeToggleButton.onClick.AddListener(() =>
            {
                UIThemeManager.Instance.ToggleTheme();
            });
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When a new scene loads, reapply the theme to all UI elements
        if (UIThemeManager.Instance != null)
        {
            UIThemeManager.Instance.ApplyTheme();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
