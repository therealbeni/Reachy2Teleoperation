using System.Collections;
using TeleopReachy;
using TMPro;
using UnityEngine;

public class DanceFlowController : MonoBehaviour
{
    [Header("Core References (drag in from scene)")]
    [Tooltip("Dance UI enable disable")]
    public GameObject DanceUIPanel;

    [Tooltip("DanceManager that actually sends trajectories to real & simulated Reachy.")]
    public DanceManager danceManager;

    [Tooltip("Grader that evaluates how well the user follows the robot.")]
    public ReachyDanceGrader grader;

    [Header("Music")]
    [Tooltip("SongSelector that knows which song is selected and how to play it.")]
    public SongSelector songSelector;

    [Header("UI Texts / TMP Objects")]
    [Tooltip("TMP text shown during the initial 'watch this dance' hint.")]
    public TMP_Text watchDanceText;

    [Tooltip("TMP text for 'Now together' phase.")]
    public TMP_Text nowTogetherText;

    [Header("Countdown TMPs (shown one after another: 3, 2, 1, GO)")]
    [Tooltip("TMP for '3'. Will be enabled/disabled by this script.")]
    public TMP_Text countdown3Text;

    [Tooltip("TMP for '2'. Will be enabled/disabled by this script.")]
    public TMP_Text countdown2Text;

    [Tooltip("TMP for '1'. Will be enabled/disabled by this script.")]
    public TMP_Text countdown1Text;

    [Tooltip("TMP for 'GO'. Will be enabled/disabled by this script.")]
    public TMP_Text countdownGoText;

    [Header("Grading Result UI")]
    [Tooltip("Root GameObject of the grading result UI (panel showing score, OK button, etc.). " +
             "Should be initially inactive and will be activated when grading is finished.")]
    public GameObject gradingResultPanel;

    [Header("Timing Settings (seconds)")]
    [Tooltip("Delay after music starts before showing the 3-2-1-GO countdown (demo phase).")]
    public float demoPreCountdownDelay = 2f;

    [Tooltip("Delay between each countdown element (3, 2, 1, GO).")]
    public float countdownStepDuration = 1f;

    [Tooltip("Delay after starting 'Now together' music before starting countdown again.")]
    public float togetherPreCountdownDelay = 2f;

    // Internal state
    private bool isSequenceRunning = false;
    private Coroutine sequenceCoroutine;

    // We listen to the global OnStopDance event from DanceManager
    private bool danceFinishedFlag = false;

    // Used by other scripts (e.g. DanceMenuVisibility) to prevent menu from being shown.
    [HideInInspector]
    public bool DontAllowMenuUnhide = false;

    private void Awake()
    {
        SafeSetActive(watchDanceText, false);
        SafeSetActive(nowTogetherText, false);
        SafeSetActive(countdown3Text, false);
        SafeSetActive(countdown2Text, false);
        SafeSetActive(countdown1Text, false);
        SafeSetActive(countdownGoText, false);

        if (gradingResultPanel != null)
            gradingResultPanel.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.StartListening(EventNames.OnStopDance, OnDanceStoppedEvent);
    }

    private void OnDisable()
    {
        EventManager.StopListening(EventNames.OnStopDance, OnDanceStoppedEvent);
    }

    // -------------------------------------------------------------------------
    // Public API for UI
    // -------------------------------------------------------------------------

    /// <summary>
    /// Hook this up to your "Start Dance" button (onClick).
    /// </summary>
    public void OnStartDanceButtonPressed()
    {
        // Hard block: do NOT allow starting again while a sequence is running.
        if (isSequenceRunning)
        {
            Debug.Log("DanceFlowController: StartDance pressed while sequence is already running. Ignoring to protect robot.");
            return;
        }

        DontAllowMenuUnhide = true;

        if (DanceUIPanel != null)
            DanceUIPanel.SetActive(false);  // hides the whole dance UI (including Start button)

        // Just in case there is an old grading popup.
        HideGradingResultPanel();

        sequenceCoroutine = StartCoroutine(DanceFlowSequence());
    }

    /// <summary>
    /// Hook this up to the "OK" button on the grading UI panel.
    /// </summary>
    public void OnGradingOkButtonPressed()
    {
        HideGradingResultPanel();

        if (DanceUIPanel != null)
            DanceUIPanel.SetActive(true);

        DontAllowMenuUnhide = false;
    }

    // -------------------------------------------------------------------------
    // Main sequence
    // -------------------------------------------------------------------------

    private IEnumerator DanceFlowSequence()
    {
        isSequenceRunning = true;

        if (danceManager == null)
        {
            Debug.LogWarning("DanceFlowController: DanceManager reference is missing. Cannot start dance.");
            isSequenceRunning = false;
            DontAllowMenuUnhide = false;
            yield break;
        }

        // PHASE 1: start music and show "watch this dance"
        if (songSelector != null)
        {
            songSelector.PlayCurrentSong();
            // TODO: Inside SongSelector or elsewhere, make sure background music is OFF while this plays.
        }
        else
        {
            Debug.LogWarning("DanceFlowController: songSelector is not assigned. No dance music will play.");
        }

        SafeSetActive(watchDanceText, true);
        yield return new WaitForSeconds(demoPreCountdownDelay);

        // PHASE 2: 3-2-1-GO (demo)
        DeactivateAllCountdowns();
        yield return CountdownRoutine();
        SafeSetActive(watchDanceText, false);

        // PHASE 3: robot demo dance (no grading)
        danceFinishedFlag = false;
        danceManager.StartPlayback();

        yield return new WaitUntil(() => danceFinishedFlag);

        // PHASE 4: demo finished, stop music and (optionally) resume background music for a moment
        if (songSelector != null)
        {
            songSelector.StopCurrentSong();
            // TODO: If you want background music between demo and together,
            // switch it ON here (e.g. backgroundMusicSource.Play()).
        }

        // PHASE 5: "Now together" + restart same track, 2s pre-countdown
        SafeSetActive(nowTogetherText, true);


        if (songSelector != null)
        {
            songSelector.PlayCurrentSong();
            // TODO: Ensure background music is OFF here again.
        }

        yield return new WaitForSeconds(togetherPreCountdownDelay);
        DeactivateAllCountdowns();
        yield return CountdownRoutine();

        // PHASE 6: start same dance again + grading
        danceFinishedFlag = false;
        danceManager.StartPlayback();

        if (grader != null)
        {
            grader.StartGrading();
            // Optional: grading text is handled by ReachyDanceGrader.scoreText (set to "Grading..." there).
        }
        else
        {
            Debug.LogWarning("DanceFlowController: Grader reference missing. No grading will be performed.");
        }

        yield return new WaitUntil(() => danceFinishedFlag);

        // PHASE 7: stop grading, stop music, resume background
        if (grader != null)
        {
            grader.StopGrading();
        }

        if (songSelector != null)
        {
            songSelector.StopCurrentSong();
            // TODO: This is the FINAL end of the flow -> switch background music ON here.
        }

        SafeSetActive(nowTogetherText, false);
        SafeSetActive(countdown3Text, false);
        SafeSetActive(countdown2Text, false);
        SafeSetActive(countdown1Text, false);
        SafeSetActive(countdownGoText, false);

        // PHASE 8: show grading outcome (popup panel)
        ShowGradingResultPanel();

        isSequenceRunning = false;
        sequenceCoroutine = null;
        // Still keep DontAllowMenuUnhide = true here so the menu stays hidden
        // until the user presses OK on the grading popup.
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private IEnumerator CountdownRoutine()
    {
        SetOnlyCountdownVisible(countdown3Text);
        yield return new WaitForSeconds(countdownStepDuration);

        DeactivateAllCountdowns();
        SetOnlyCountdownVisible(countdown2Text);
        yield return new WaitForSeconds(countdownStepDuration);

        DeactivateAllCountdowns();
        SetOnlyCountdownVisible(countdown1Text);
        yield return new WaitForSeconds(countdownStepDuration);

        DeactivateAllCountdowns();
        SetOnlyCountdownVisible(countdownGoText);
        yield return new WaitForSeconds(countdownStepDuration);

        SetOnlyCountdownVisible(null);
        DeactivateAllCountdowns();
    }

    private void SetOnlyCountdownVisible(TMP_Text active)
    {
        SafeSetActive(countdown3Text, active == countdown3Text);
        SafeSetActive(countdown2Text, active == countdown2Text);
        SafeSetActive(countdown1Text, active == countdown1Text);
        SafeSetActive(countdownGoText, active == countdownGoText);
    }

    private void DeactivateAllCountdowns()
    {
        SafeSetActive(watchDanceText, false);
        SafeSetActive(nowTogetherText, false);
        SafeSetActive(countdown3Text, false);
        SafeSetActive(countdown2Text, false);
        SafeSetActive(countdown1Text, false);
        SafeSetActive(countdownGoText, false);
    }

    private void SafeSetActive(TMP_Text text, bool active)
    {
        if (text != null && text.gameObject.activeSelf != active)
        {
            text.gameObject.SetActive(active);
        }
    }

    private void ShowGradingResultPanel()
    {
        if (gradingResultPanel != null)
        {
            gradingResultPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("DanceFlowController: gradingResultPanel is not assigned.");
        }
    }

    private void HideGradingResultPanel()
    {
        if (gradingResultPanel != null && gradingResultPanel.activeSelf)
        {
            gradingResultPanel.SetActive(false);
        }
    }

    private void OnDanceStoppedEvent()
    {
        danceFinishedFlag = true;
    }
}
