using UnityEngine;

public class JukeboxController : MonoBehaviour
{
    public void PlayNextTrack()
    {
        // Skip to next song
        MusicSystem.Instance.Next();

        // Update UI highlight if menu is open
        if (MusicMenuUIManager.Instance != null)
        {
            MusicMenuUIManager.Instance.HighlightTrack(MusicSystem.Instance.CurrentIndex);
        }
    }
}
