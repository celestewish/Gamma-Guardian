using UnityEngine;

public class BacteriaDot : MonoBehaviour
{
    private RectTransform rectTransform;

    void Awake() { rectTransform = GetComponent<RectTransform>(); }

    public void SetPosition(Vector2 pos)
    {
        rectTransform.anchoredPosition = pos;
    }
}
