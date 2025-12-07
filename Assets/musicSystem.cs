using UnityEngine;

public enum MusicMode { Background, Dance }

public class MusicSystem : MonoBehaviour
{
    public static MusicSystem Instance;

    [Header("Music Tracks")]
    public AudioClip backgroundTrack;     // Old tracks[0]
    public AudioClip[] danceTracks;       // Old tracks[1..n]

    private AudioSource audioSource;
    private int currentDanceIndex = 0;

    public MusicMode Mode { get; private set; } = MusicMode.Background;

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
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        PlayBackground();
    }

    //--------------------------------------
    // Background mode
    //--------------------------------------

    public void PlayBackground()
    {
        if (Mode == MusicMode.Background && audioSource.clip == backgroundTrack && audioSource.isPlaying)
        {
            return;
        }

        Mode = MusicMode.Background;

        if (backgroundTrack == null)
        {
            Debug.LogWarning("No background track assigned.");
            return;
        }

        audioSource.clip = backgroundTrack;
        audioSource.loop = true;
        audioSource.Play();
    }


    //--------------------------------------
    // Dance mode
    //--------------------------------------

    public void StartDance()
    {
        if (danceTracks.Length == 0)
        {
            Debug.LogWarning("No dance tracks assigned.");
            return;
        }

        Mode = MusicMode.Dance;
        audioSource.loop = false;

        currentDanceIndex = 0;
        audioSource.clip = danceTracks[currentDanceIndex];
        audioSource.Play();
    }

    public void StopDance()
    {
        PlayBackground();
    }

    public void NextDance()
    {
        if (Mode != MusicMode.Dance) return;

        currentDanceIndex = (currentDanceIndex + 1) % danceTracks.Length;
        audioSource.clip = danceTracks[currentDanceIndex];
        audioSource.Play();
    }

    public void PreviousDance()
    {
        if (Mode != MusicMode.Dance) return;

        currentDanceIndex = (currentDanceIndex - 1 + danceTracks.Length) % danceTracks.Length;
        audioSource.clip = danceTracks[currentDanceIndex];
        audioSource.Play();
    }

    //--------------------------------------
    // Mute functionality (unchanged)
    //--------------------------------------

    public void ToggleMute()
    {
        audioSource.mute = !audioSource.mute;
    }

    public void SetMute(bool mute)
    {
        audioSource.mute = mute;
    }

    public int GetCurrentDanceIndex()
    {
        return currentDanceIndex;
    }

    public bool IsMuted => audioSource.mute;
}
