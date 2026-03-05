using UnityEngine;
using UnityEngine.UI;

public class MinimapRadar : MonoBehaviour
{
    [Header("UI Dots")]
    public Image playerDot;
    public GameObject bacteriaDotPrefab;
    public Transform player;

    [Header("Radar")]
    public float radarRadius = 100f;
    public float uiScale = 0.3f;

    private RectTransform rectTransform;
    private Camera uiCam;
    private BacteriaDot[] bacteriaDots = new BacteriaDot[10];
    private int activeDotCount = 0;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        uiCam = rectTransform.GetComponentInParent<Canvas>().worldCamera;
    }

    void Start()
    {
        for (int i = 0; i < bacteriaDots.Length; i++)
        {
            GameObject dot = Instantiate(bacteriaDotPrefab, transform);
            bacteriaDots[i] = dot.GetComponent<BacteriaDot>();
            bacteriaDots[i].gameObject.SetActive(false);
        }
        if (playerDot != null)
        {
            playerDot.gameObject.SetActive(true);
            playerDot.color = Color.white;
            playerDot.rectTransform.anchoredPosition = Vector2.zero;
            playerDot.rectTransform.localScale = Vector3.one;
            Debug.Log("PlayerDot forced: pos=" + playerDot.rectTransform.anchoredPosition +
                      " color=" + playerDot.color);
        }
    }

    void Update()
    {
        if (!GameManager.Instance.levelRunning) return;

        UpdateBacteriaDots();
    }

    void UpdateBacteriaDots()
    {
        activeDotCount = 0;
        BacteriaAI[] bacteria = Object.FindObjectsByType<BacteriaAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var bacteriaAI in bacteria)
        {
            if (activeDotCount >= bacteriaDots.Length) break;

            Vector2 relativePos2D = bacteriaAI.transform.position - player.position;
            if (relativePos2D.magnitude > radarRadius) continue;

            Vector2 uiPos = relativePos2D * uiScale;
            bacteriaDots[activeDotCount].SetPosition(uiPos);
            bacteriaDots[activeDotCount].gameObject.SetActive(true);
            activeDotCount++;
        }
    }

}
