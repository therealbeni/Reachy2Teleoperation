using UnityEngine;

public class SceneMusicSwitcher : MonoBehaviour
{
    public bool useDanceMusic = false;

    void Start()
    {
        if (MusicSystem.Instance == null)
        {
            Debug.LogWarning("MusicSystem not found in scene.");
            return;
        }

        if (useDanceMusic)
        {
            Debug.Log("SceneMusicSwitcher: switching to DANCE mode.");
            MusicSystem.Instance.StartDance();
        }
        else
        {
            Debug.Log("SceneMusicSwitcher: switching to BACKGROUND mode.");
            MusicSystem.Instance.PlayBackground();
        }
    }
}
