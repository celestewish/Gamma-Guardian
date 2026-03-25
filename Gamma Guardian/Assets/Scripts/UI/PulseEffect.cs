using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    [Header("Bulletproof Pulse")]
    public float speed = 2f;
    public AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0.95f, 1, 1.35f);  // Inspector curve!

    private SpriteRenderer sr;
    [HideInInspector] public bool isPulsing;

    [Header("Size")]
    [SerializeField] private float baseSize = 1f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) { enabled = false; return; }
        sr.sortingOrder = 2;  // Always above parent [web:120]
        sr.material = new Material(sr.material);  // Unique instance
        SetPulseActive(false);
    }

    void Update()
    {
        if (!isPulsing) return;
        float t = pulseCurve.Evaluate((Time.time * speed) % 1f);
        transform.localScale = Vector3.one * (baseSize * t);  // <- Multiply!
        sr.color = new Color(0, 1, 1, 0.25f);
    }

    public void SetPulseActive(bool active)
    {
        isPulsing = active;
        sr.enabled = active;
    }
}