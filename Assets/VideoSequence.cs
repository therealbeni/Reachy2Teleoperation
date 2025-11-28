using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoSequence : MonoBehaviour
{
    public VideoPlayer player;
    public VideoClip[] clips;
    private int index = 0;

    public void NextVideo()
    {
        index++;
        if (index >= clips.Length)
            index = 0;  // or do nothing if you don't want looping

        player.clip = clips[index];
        player.Play();
    }

    public void PrevVideo()
    {
        index--;
        if (index < 0)
            index = clips.Length - 1;

        player.clip = clips[index];
        player.Play();
    }
}
