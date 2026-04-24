using UnityEngine;

public class UIPulseHighlight : MonoBehaviour
{
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private float pulseSpeed = 2.2f;
    [SerializeField] private float pulseAmount = 0.08f;

    private Vector3 baseScale = Vector3.one;

    private void Awake()
    {
        if (targetRect == null)
            targetRect = GetComponent<RectTransform>();

        baseScale = targetRect.localScale;
    }

    private void OnEnable()
    {
        if (targetRect != null)
            targetRect.localScale = baseScale;
    }

    private void Update()
    {
        if (targetRect == null) return;

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f) * pulseAmount;
        targetRect.localScale = baseScale * pulse;
    }

    public void SetBaseScale(Vector3 newScale)
    {
        baseScale = newScale;
        if (targetRect != null)
            targetRect.localScale = baseScale;
    }

    public void ResetPulse()
    {
        if (targetRect != null)
            targetRect.localScale = baseScale;
    }
}