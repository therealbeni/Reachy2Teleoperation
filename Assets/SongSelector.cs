using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SongSelector : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text songLabel;          // The middle text

    [Header("Songs")]
    public List<string> songs = new List<string>();

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
        songLabel.text = songs[currentIndex];
    }
}
