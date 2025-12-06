using UnityEngine;
using UnityEngine.UI;

public class MuteButtonBinder : MonoBehaviour
{
    void Start()
    {
        Button button = GetComponent<Button>();

        // Remove old listeners to avoid duplicates
        button.onClick.RemoveAllListeners();

        // Bind to the MusicSystem singleton when clicked
        button.onClick.AddListener(() =>
        {
            if (MusicSystem.Instance != null)
                MusicSystem.Instance.ToggleMute();
            else
                Debug.LogError("MusicSystem.Instance not found!");
        });
    }
}
