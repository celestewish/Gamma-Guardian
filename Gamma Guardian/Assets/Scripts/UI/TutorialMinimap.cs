using UnityEngine;
using UnityEngine.UI;

public class TutorialMinimap : MonoBehaviour
{
    [Header("UI Dots")]
    public Image playerDot;
    public GameObject bacteriaDotPrefab;
    public Transform player;

    [Header("Radar")]
    public float radarRadius = 50f;
    public float uiScale = 1f;

    private RectTransform rectTransform;
    private BacteriaDot[] bacteriaDots = new BacteriaDot[10];

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        for (int i = 0; i < bacteriaDots.Length; i++)
        {
            GameObject dot = Instantiate(bacteriaDotPrefab, transform);
            bacteriaDots[i] = dot.GetComponent<BacteriaDot>();
            bacteriaDots[i].gameObject.SetActive(false);
        }
        if (playerDot)
        {
            playerDot.gameObject.SetActive(true);
            playerDot.color = Color.white;
            playerDot.rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    public void SpawnDemoDots(Vector2[] relativePositions)
    {
        for (int i = 0; i < relativePositions.Length && i < bacteriaDots.Length; i++)
        {
            Vector2 relPos = relativePositions[i];
            if (relPos.magnitude > radarRadius)
            {
                relPos = relPos.normalized * radarRadius;
            }

            Vector2 uiPos = relPos * uiScale;
            bacteriaDots[i].SetPosition(uiPos);
            bacteriaDots[i].gameObject.SetActive(true);
        }
        for (int i = relativePositions.Length; i < bacteriaDots.Length; i++)
        {
            bacteriaDots[i].gameObject.SetActive(false);
        }
    }
}