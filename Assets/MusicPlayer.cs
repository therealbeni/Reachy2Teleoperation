using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;

    public AudioClip[] tracks;
    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayTrack(int index)
    {
        if (index < 0 || index >= tracks.Length)
            return;

        audioSource.clip = tracks[index];
        audioSource.Play();

        MusicMenuUIManager.Instance.HighlightTrack(index);
    }
}
