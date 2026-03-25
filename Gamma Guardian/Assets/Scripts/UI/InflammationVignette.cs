using UnityEngine;
using UnityEngine.UI;

public class InflammationVignette : MonoBehaviour
{
    [SerializeField] private Image vignetteImage;
    [SerializeField] private float maxAlpha = 0.6f;

    // Call this from your inflammation system with a value 0–1
    public void SetInflammation(float normalizedInflammation)
    {
        Color c = vignetteImage.color;
        c.a = Mathf.Lerp(0f, maxAlpha, normalizedInflammation);
        vignetteImage.color = c;
    }
}