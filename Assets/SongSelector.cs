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
    [Tooltip("AudioSource that will play the selected dance track (also used for preview).")]
    public AudioSource danceAudioSource;

    [Tooltip("Audio clips in the SAME order as 'songs'.")]
    public List<AudioClip> songClips = new List<AudioClip>();

    [Tooltip("Json motion recordings in the SAME order as 'songs'")]
    public List<string> recordingFileNames = new List<string>();

    int currentIndex = 0;

    void Awake()
    {
        if (danceAudioSource == null)
            danceAudioSource = FindObjectOfType<AudioSource>();
    }

    void Start()
    {
        if (songs.Count > 0)
        {
            UpdateLabel();
            PlayPreview();   // start preview of the first song
        }
    }

    public string GetCurrentRecordingFileName()
    {
        if (recordingFileNames == null || recordingFileNames.Count == 0) return null;
        int idx = Mathf.Clamp(currentIndex, 0, recordingFileNames.Count - 1);
        return recordingFileNames[idx];
    }


    public void NextSong()
    {
        if (songs.Count == 0) return;

        currentIndex = (currentIndex + 1) % songs.Count;  // wrap around
        UpdateLabel();
        PlayPreview();   // update preview to new song
    }

    public void PreviousSong()
    {
        if (songs.Count == 0) return;

        currentIndex = (currentIndex - 1 + songs.Count) % songs.Count; // wrap around
        UpdateLabel();
        PlayPreview();   // update preview to new song
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

    // ---------- PREVIEW (same AudioSource) ----------

    public void PlayPreview()
    {
        if (danceAudioSource == null) return;

        AudioClip clip = GetCurrentSongClip();
        if (clip == null) return;

        danceAudioSource.Stop();
        danceAudioSource.clip = clip;
        danceAudioSource.loop = true;  // preview loops in menu
        danceAudioSource.time = 0f;
        danceAudioSource.Play();
    }

    public void StopPreview()
    {
        if (danceAudioSource != null && danceAudioSource.isPlaying)
            danceAudioSource.Stop();
    }

    // ---------- DANCE PLAYBACK (called from DanceFlowController) ----------

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

        // make sure preview is not looping anymore
        StopPreview();

        // TODO: Switch OFF background music here before starting dance music.

        danceAudioSource.clip = clip;
        danceAudioSource.loop = false;   // one shot; DanceFlowController decides when to replay
        danceAudioSource.time = 0f;      // always from start for demo & together phases
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
