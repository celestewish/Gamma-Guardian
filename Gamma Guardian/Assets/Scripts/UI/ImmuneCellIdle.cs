using UnityEngine;

public class ImmuneCellRoam : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 40f;
    public float changeTargetEvery = 2.5f;
    public float targetPadding = 20f;

    [Header("Float")]
    public float bobSpeed = 2f;
    public float bobAmplitude = 6f;

    [Header("Bounds")]
    public RectTransform boundsRect;

    private RectTransform rt;
    private Vector2 targetPos;
    private Vector2 basePos;
    private float targetTimer;
    private float bobOffset;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        basePos = rt.anchoredPosition;
        bobOffset = Random.Range(0f, 999f);
        PickNewTarget();
    }

    void Update()
    {
        targetTimer += Time.deltaTime;
        if (targetTimer >= changeTargetEvery)
        {
            PickNewTarget();
            targetTimer = 0f;
        }

        Vector2 currentPos = rt.anchoredPosition;
        Vector2 toTarget = targetPos - currentPos;
        Vector2 moveDir = toTarget.normalized;

        Vector2 nextPos = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);

        float bob = Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobAmplitude;
        nextPos.y += bob * Time.deltaTime;

        rt.anchoredPosition = ClampToBounds(nextPos);
    }

    void PickNewTarget()
    {
        Rect r = boundsRect.rect;
        float minX = r.xMin + targetPadding;
        float maxX = r.xMax - targetPadding;
        float minY = r.yMin + targetPadding;
        float maxY = r.yMax - targetPadding;

        targetPos = new Vector2(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY)
        );
    }

    Vector2 ClampToBounds(Vector2 pos)
    {
        if (boundsRect == null) return pos;

        Rect r = boundsRect.rect;
        Vector2 halfSize = rt.rect.size * 0.5f;

        float minX = r.xMin + halfSize.x;
        float maxX = r.xMax - halfSize.x;
        float minY = r.yMin + halfSize.y;
        float maxY = r.yMax - halfSize.y;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        return pos;
    }
}