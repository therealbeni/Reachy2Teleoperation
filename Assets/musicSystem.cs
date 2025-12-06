using UnityEngine;

public class MusicSystem : MonoBehaviour
{
    public static MusicSystem Instance;

    public AudioClip[] tracks;

    private AudioSource audioSource;
    private int currentIndex;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        audioSource.loop = true;        // << always loop the track
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        // Play first track automatically
        if (tracks != null && tracks.Length > 0)
        {
            PlayIndex(0);
        }
    }

    public int CurrentIndex => currentIndex;

    public void PlayIndex(int index)
    {
        if (index < 0 || index >= tracks.Length)
            return;

        currentIndex = index;

        audioSource.clip = tracks[currentIndex];
        audioSource.Play();
    }

    public void Next()
    {
        if (tracks.Length == 0)
            return;

        int next = (currentIndex + 1) % tracks.Length;
        PlayIndex(next);
    }

    public void Previous()
    {
        if (tracks.Length == 0)
            return;

        int prev = (currentIndex - 1 + tracks.Length) % tracks.Length;
        PlayIndex(prev);
    }

    public void ToggleMute()
    {
        Debug.Log("Mute button pressed!");
        audioSource.mute = !audioSource.mute;
    }

    public void SetMute(bool mute)
    {
        audioSource.mute = mute;
    }

    public bool IsMuted => audioSource.mute;

}
