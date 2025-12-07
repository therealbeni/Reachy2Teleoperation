using UnityEngine;

public class JukeboxController : MonoBehaviour
{
    public void PlayNextTrack()
    {
        // Skip to next song
        MusicSystem.Instance.NextDance();

        // Update UI highlight if menu is open
        if (MusicMenuUIManager.Instance != null)
        {
            MusicMenuUIManager.Instance.HighlightTrack(MusicSystem.Instance.GetCurrentDanceIndex());
        }
    }
}
