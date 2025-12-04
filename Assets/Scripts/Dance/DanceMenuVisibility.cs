using System.Collections;
using UnityEngine;

public class DanceMenuVisibility : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeTime = 0.25f;

    private CanvasGroup canvasGroup;
    private bool isVisible = true;
    private Coroutine currentFade;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Start visible
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        isVisible = true;
    }

    private void Update()
    {
        // --- Keyboard debug (Editor only, like in your DanceRecorder) ---
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("DanceMenuVisibility: Editor key X pressed -> ToggleVisibility()");
            ToggleVisibility();
        }
#endif

        // --- Quest 3 controller X button ---
        // You used OVRInput.RawButton.B / A in DanceRecorder,
        // so here we use RawButton.X for the X button.
        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            Debug.Log("DanceMenuVisibility: OVR X pressed -> ToggleVisibility()");
            ToggleVisibility();
        }
    }

    public void ToggleVisibility()
    {
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
        }

        currentFade = StartCoroutine(FadeTo(!isVisible));
    }

    private IEnumerator FadeTo(bool show)
    {
        isVisible = show;

        float startAlpha = canvasGroup.alpha;
        float endAlpha = show ? 1f : 0f;

        float t = 0f;

        if (show)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / fadeTime);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, lerp);

            yield return null;
        }

        canvasGroup.alpha = endAlpha;

        if (!show)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        currentFade = null;
    }
}
