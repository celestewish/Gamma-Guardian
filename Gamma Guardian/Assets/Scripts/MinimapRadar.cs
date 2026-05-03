using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapRadar : MonoBehaviour
{
    [Header("UI Dots")]
    public Image playerDot;

    [Tooltip("Prefab used for bacteria dots")]
    public GameObject bacteriaDotPrefab;

    [Tooltip("Prefab used for cytokine dots")]
    public GameObject cytokineDotPrefab;

    [Tooltip("Prefab used for immune cell dots")]
    public GameObject immuneCellDotPrefab;

    public Transform player;

    [Header("Radar")]
    public float radarRadius = 100f;
    public float uiScale = 0.3f;

    private RectTransform rectTransform;
    private Camera uiCam;

    // Pools
    private BacteriaDot[] bacteriaDots = new BacteriaDot[10];
    private BacteriaDot[] cytokineDots = new BacteriaDot[10];
    private BacteriaDot[] immuneCellDots = new BacteriaDot[10];

    private int activeBacteriaDotCount = 0;
    private int activeCytokineDotCount = 0;
    private int activeImmuneDotCount = 0;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        uiCam = rectTransform.GetComponentInParent<Canvas>().worldCamera;
    }

    void Start()
    {
        // Bacteria pool
        for (int i = 0; i < bacteriaDots.Length; i++)
        {
            GameObject dot = Instantiate(bacteriaDotPrefab, transform);
            bacteriaDots[i] = dot.GetComponent<BacteriaDot>();
            bacteriaDots[i].gameObject.SetActive(false);
        }

        // Cytokine pool
        for (int i = 0; i < cytokineDots.Length; i++)
        {
            GameObject dot = Instantiate(cytokineDotPrefab, transform);
            cytokineDots[i] = dot.GetComponent<BacteriaDot>();
            cytokineDots[i].gameObject.SetActive(false);
        }

        // Immune cell pool
        for (int i = 0; i < immuneCellDots.Length; i++)
        {
            GameObject dot = Instantiate(immuneCellDotPrefab, transform);
            immuneCellDots[i] = dot.GetComponent<BacteriaDot>();
            immuneCellDots[i].gameObject.SetActive(false);
        }

        if (playerDot != null)
        {
            playerDot.gameObject.SetActive(true);
            playerDot.color = Color.white;
            playerDot.rectTransform.anchoredPosition = Vector2.zero;
            playerDot.rectTransform.localScale = Vector3.one;
        }
    }

    void Update()
    {
        UpdateBacteriaDots();
        UpdateCytokineDots();
        UpdateImmuneCellDots();
    }

    void UpdateBacteriaDots()
    {
        activeBacteriaDotCount = 0;

        BacteriaAI[] bacteria = Object.FindObjectsByType<BacteriaAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        TutorialBacteria[] tutorialBacteria = Object.FindObjectsByType<TutorialBacteria>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        List<Vector3> allPositions = new List<Vector3>();
        foreach (var b in bacteria) allPositions.Add(b.transform.position);
        foreach (var t in tutorialBacteria) allPositions.Add(t.transform.position);

        foreach (var pos in allPositions)
        {
            if (activeBacteriaDotCount >= bacteriaDots.Length) break;

            Vector2 relativePos2D = pos - player.position;
            if (relativePos2D.magnitude > radarRadius) continue;

            Vector2 uiPos = relativePos2D * uiScale;
            bacteriaDots[activeBacteriaDotCount].SetPosition(uiPos);
            bacteriaDots[activeBacteriaDotCount].gameObject.SetActive(true);
            activeBacteriaDotCount++;
        }

        for (int i = activeBacteriaDotCount; i < bacteriaDots.Length; i++)
        {
            if (bacteriaDots[i].gameObject.activeSelf)
                bacteriaDots[i].gameObject.SetActive(false);
        }
    }

    void UpdateCytokineDots()
    {
        activeCytokineDotCount = 0;

        TutorialCytokine[] tutorialCytokines =
            Object.FindObjectsByType<TutorialCytokine>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        CytokinesScript[] cytokines =
            Object.FindObjectsByType<CytokinesScript>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        List<Vector3> allPositions = new List<Vector3>();
        foreach (var c in tutorialCytokines) allPositions.Add(c.transform.position);
        foreach (var c in cytokines) allPositions.Add(c.transform.position);

        foreach (var pos in allPositions)
        {
            if (activeCytokineDotCount >= cytokineDots.Length) break;

            Vector2 relativePos2D = pos - player.position;
            if (relativePos2D.magnitude > radarRadius) continue;

            Vector2 uiPos = relativePos2D * uiScale;
            cytokineDots[activeCytokineDotCount].SetPosition(uiPos);
            cytokineDots[activeCytokineDotCount].gameObject.SetActive(true);
            activeCytokineDotCount++;
        }

        for (int i = activeCytokineDotCount; i < cytokineDots.Length; i++)
        {
            if (cytokineDots[i].gameObject.activeSelf)
                cytokineDots[i].gameObject.SetActive(false);
        }
    }

    void UpdateImmuneCellDots()
    {
        activeImmuneDotCount = 0;

        ImmuneCellScript[] immuneCells =
            Object.FindObjectsByType<ImmuneCellScript>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var cell in immuneCells)
        {
            if (activeImmuneDotCount >= immuneCellDots.Length) break;

            Vector2 relativePos2D = cell.transform.position - player.position;
            if (relativePos2D.magnitude > radarRadius) continue;

            Vector2 uiPos = relativePos2D * uiScale;
            immuneCellDots[activeImmuneDotCount].SetPosition(uiPos);
            immuneCellDots[activeImmuneDotCount].gameObject.SetActive(true);
            activeImmuneDotCount++;
        }

        for (int i = activeImmuneDotCount; i < immuneCellDots.Length; i++)
        {
            if (immuneCellDots[i].gameObject.activeSelf)
                immuneCellDots[i].gameObject.SetActive(false);
        }
    }
}