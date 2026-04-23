using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class InflammationWarningUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float flashTime = 0.12f;
    [SerializeField] private int flashCount = 3;
    [SerializeField] private float holdTime = 0.2f;
    [SerializeField] private float fadeOutTime = 0.4f;

    [Header("Alpha")]
    [SerializeField] private float maxAlpha = 1f;
    [SerializeField] private float flashLowAlpha = 0.35f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void PlayWarning()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(WarningSequence());
    }

    public void HideInstant()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = null;
        canvasGroup.alpha = 0f;
    }

    private IEnumerator WarningSequence()
    {
        yield return FadeTo(maxAlpha, fadeInTime);

        for (int i = 0; i < flashCount; i++)
        {
            yield return FadeTo(flashLowAlpha, flashTime);
            yield return FadeTo(maxAlpha, flashTime);
        }

        yield return new WaitForSeconds(holdTime);
        yield return FadeTo(0f, fadeOutTime);

        currentRoutine = null;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}