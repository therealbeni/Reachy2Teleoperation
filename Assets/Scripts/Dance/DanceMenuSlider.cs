using System.Collections;
using UnityEngine;

public class DanceMenuSliderLocal : MonoBehaviour
{
    [Header("Slide settings")]
    public float slideDistanceWorld = 0.1f;   // how far to move along the panel's local X (right) in world units
    public float duration = 0.25f;

    RectTransform rect;
    CanvasGroup canvasGroup;

    Vector3 closedLocalPos;
    Vector3 openLocalPos;

    bool isOpen = false;
    Coroutine currentRoutine;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // CLOSED = current position (as set in editor)
        closedLocalPos = rect.localPosition;

        // compute OPEN position:
        // 1) get current world position
        // 2) move along the panel's local X axis in world space (rect.right)
        // 3) convert that back into local space of the parent
        Transform parent = rect.parent;
        Vector3 closedWorldPos = rect.position;
        Vector3 openWorldPos = closedWorldPos + rect.right * slideDistanceWorld;
        openLocalPos = parent.InverseTransformPoint(openWorldPos);

        // start closed & invisible
        rect.localPosition = closedLocalPos;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Toggle()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(SlideAndFade(!isOpen));
    }

    IEnumerator SlideAndFade(bool opening)
    {
        isOpen = opening;

        float time = 0f;

        Vector3 startPos = rect.localPosition;
        Vector3 endPos = opening ? openLocalPos : closedLocalPos;

        float startAlpha = canvasGroup.alpha;
        float endAlpha = opening ? 1f : 0f;

        if (opening)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            rect.localPosition = Vector3.Lerp(startPos, endPos, t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        rect.localPosition = endPos;
        canvasGroup.alpha = endAlpha;

        if (!opening)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        currentRoutine = null;
    }
}
