using UnityEngine;

public class JukeboxMusicSwitcher : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] musicTracks;

    private int currentTrack = 0;

    // Call this method when the player clicks or interacts with the jukebox
    public void PlayNextTrack()
    {
        if (musicTracks.Length == 0) return;

        currentTrack = (currentTrack + 1) % musicTracks.Length;

        audioSource.clip = musicTracks[currentTrack];
        audioSource.Play();
    }
}
