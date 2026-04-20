using UnityEngine;
using UnityEngine.UI;

public class InflammationBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient colorGradient;

    public void SetInflammation(float value)
    {
        value = Mathf.Clamp01(value);
        fillImage.fillAmount = value;
        fillImage.color = colorGradient.Evaluate(value);
    }
}