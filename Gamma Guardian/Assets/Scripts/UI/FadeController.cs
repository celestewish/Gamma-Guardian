using System.Collections;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;  // Adjustable in Inspector
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Call to fade in (black overlay appears)
    public void FadeIn()
    {
        StartCoroutine(DoFade(1f));
    }

    // Call to fade out (overlay disappears)
    public void FadeOut()
    {
        StartCoroutine(DoFade(0f));
    }

    private IEnumerator DoFade(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}