using UnityEngine;
using UnityEngine.UI;

public class ArrowPointer : MonoBehaviour
{
    [Header("UI References")]
    public Image circleImage;
    public Image arrowImage;
    public float edgePadding = 250f;

    [Header("Settings")]
    public float minDistanceToHide = 5f;
    public LayerMask bacteriaLayer = -1;

    private RectTransform rectTransform;
    private Camera cam;
    public Transform player;
    private BacteriaAI[] bacteriaAI;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cam = Camera.main;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        bacteriaAI = Object.FindObjectsByType<BacteriaAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        gameObject.SetActive(true);
    }

    void LateUpdate()
    {
        if (player == null || !GameManager.Instance.levelRunning || GameManager.Instance.isPaused)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        UpdateArrow();
    }

    void UpdateArrow()
    {
        Transform nearest = GetNearestBacteria();
        if (nearest == null)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        Vector3 screenPos = cam.WorldToViewportPoint(nearest.position);
        float distToPlayer = Vector3.Distance(player.position, nearest.position);

        bool inFront = screenPos.z > 0;
        bool onViewport = screenPos.x >= 0f && screenPos.x <= 1f && screenPos.y >= 0f && screenPos.y <= 1f;
        if (inFront && onViewport && distToPlayer < minDistanceToHide)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        canvasGroup.alpha = 1f;

        // Compute direction in viewport space (centered at 0.5, 0.5)
        Vector2 viewportDir = new Vector2(screenPos.x - 0.5f, screenPos.y - 0.5f);

        // If the target is behind the camera, flip the direction
        if (screenPos.z < 0) viewportDir = -viewportDir;

        viewportDir.Normalize();

        // MOVE the whole locator toward the edge in the viewport direction
        RectTransform parentRect = (RectTransform)transform.parent;
        float screenRadius = Mathf.Min(parentRect.rect.width, parentRect.rect.height) * 0.45f;
        Vector2 edgeOffset = viewportDir * (screenRadius - edgePadding);
        rectTransform.anchoredPosition = edgeOffset;

        // ROTATE circle using the same viewport-space direction so the arrow
        // consistently points toward the target from the player's screen center
        float angle = Mathf.Atan2(viewportDir.y, viewportDir.x) * Mathf.Rad2Deg - 90f;
        circleImage.rectTransform.localEulerAngles = new Vector3(0, 0, angle);

        // IMPORTANT (done in Inspector, not code):
        // - Arrow is child of circleImage
        // - Arrow RectTransform anchoredPosition = (0, radius) so it sits at top of circle
        // - Arrow localEulerAngles = (0, 0, 0)
    }

    Transform GetNearestBacteria()
    {
        Transform nearest = null;
        float closestDist = float.MaxValue;

        foreach (var bacteria in bacteriaAI)
        {
            if (bacteria == null) continue;
            if (!bacteria.gameObject.activeInHierarchy) continue;
            float dist = Vector3.Distance(player.position, bacteria.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                nearest = bacteria.transform;
            }
        }
        return nearest;
    }
}