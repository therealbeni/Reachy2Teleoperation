using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SongSelector : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text songLabel;          // The middle text

    [Header("Songs (names for UI)")]
    public List<string> songs = new List<string>();

    [Header("Audio")]
    [Tooltip("AudioSource that will play the selected dance track.")]
    public AudioSource danceAudioSource;

    [Tooltip("Audio clips in the SAME order as 'songs'.")]
    public List<AudioClip> songClips = new List<AudioClip>();

    int currentIndex = 0;

    void Start()
    {
        if (songs.Count > 0)
            UpdateLabel();
    }

    public void NextSong()
    {
        if (songs.Count == 0) return;

        currentIndex = (currentIndex + 1) % songs.Count;  // wrap around
        UpdateLabel();
    }

    public void PreviousSong()
    {
        if (songs.Count == 0) return;

        currentIndex = (currentIndex - 1 + songs.Count) % songs.Count; // wrap around
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (songLabel != null && currentIndex >= 0 && currentIndex < songs.Count)
            songLabel.text = songs[currentIndex];
    }

    // --- Helpers used by DanceFlowController ---

    public string GetCurrentSongName()
    {
        if (songs == null || songs.Count == 0) return null;
        return songs[Mathf.Clamp(currentIndex, 0, songs.Count - 1)];
    }

    public AudioClip GetCurrentSongClip()
    {
        if (songClips == null || songClips.Count == 0) return null;
        int idx = Mathf.Clamp(currentIndex, 0, songClips.Count - 1);
        return songClips[idx];
    }

    public void PlayCurrentSong()
    {
        if (danceAudioSource == null)
        {
            Debug.LogWarning("SongSelector: danceAudioSource is not assigned.");
            return;
        }

        AudioClip clip = GetCurrentSongClip();
        if (clip == null)
        {
            Debug.LogWarning("SongSelector: current song clip is null or not set up.");
            return;
        }

        // TODO: Switch OFF background music here before starting dance music.
        // Example (pseudo-code):
        //   backgroundMusicSource.Stop();

        danceAudioSource.clip = clip;
        danceAudioSource.loop = false;   // One shot; we control when to replay
        danceAudioSource.Play();
    }

    public void StopCurrentSong()
    {
        if (danceAudioSource != null && danceAudioSource.isPlaying)
        {
            danceAudioSource.Stop();
        }

        // TODO: Optionally switch background music back ON
        // at the END of the whole flow from outside.
    }
}
