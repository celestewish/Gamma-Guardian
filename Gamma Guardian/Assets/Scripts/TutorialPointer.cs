using UnityEngine;
using UnityEngine.UI;

public class TutorialPointer : MonoBehaviour
{
    [Header("UI References")]
    public Image circleImage;
    public Image arrowImage;
    public float edgePadding = 50f;

    [Header("Settings")]
    public float minDistanceToHide = 5f;

    [Header("References")]
    public Transform player;

    private RectTransform rectTransform;
    private Camera cam;
    private CanvasGroup canvasGroup;

    private static readonly string[] targetTags = { "Bacteria", "Cytokines", "ImmuneCell" };

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cam = Camera.main;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void LateUpdate()
    {
        if (player == null)
        {
            canvasGroup.alpha = 0f;
            return;
        }
        UpdateArrow();
    }

    void UpdateArrow()
    {
        Transform nearest = GetNearestTarget();
        if (nearest == null)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        Vector3 dirToTarget = (nearest.position - player.position).normalized;
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

        Vector2 viewportDir = new Vector2(screenPos.x - 0.5f, screenPos.y - 0.5f).normalized;
        RectTransform parentRect = (RectTransform)transform.parent;
        float screenRadius = Mathf.Min(parentRect.rect.width, parentRect.rect.height) * 0.45f;
        Vector2 edgeOffset = viewportDir * (screenRadius - edgePadding);
        rectTransform.anchoredPosition = edgeOffset;

        float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg - 90f;
        circleImage.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
    }

    Transform GetNearestTarget()
    {
        Collider2D[] allColliders = Physics2D.OverlapCircleAll(player.position, 20f);
        Transform nearest = null;
        float closestDist = float.MaxValue;

        foreach (var col in allColliders)
        {
            foreach (string tag in targetTags)
            {
                if (col.CompareTag(tag))
                {
                    float dist = Vector3.Distance(player.position, col.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        nearest = col.transform;
                    }
                    break;
                }
            }
        }
        return nearest;
    }
}
