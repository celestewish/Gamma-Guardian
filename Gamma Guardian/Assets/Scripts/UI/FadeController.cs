using DG.Tweening;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        RefreshCanvasGroup();
    }

    public void RefreshCanvasGroup()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            Debug.LogWarning("No CanvasGroup found on " + gameObject.name);
    }

    public Tween FadeIn()
    {
        RefreshCanvasGroup();  // Always get fresh reference
        if (canvasGroup == null) return null;

        DOTween.Kill(canvasGroup);  // Kill any old tweens on this CanvasGroup
        return canvasGroup.DOFade(1f, 0.5f).SetLink(gameObject);
    }

    public Tween FadeOut()
    {
        RefreshCanvasGroup();
        if (canvasGroup == null) return null;

        DOTween.Kill(canvasGroup);
        return canvasGroup.DOFade(0f, 0.5f).SetLink(gameObject);
    }
}