using UnityEngine;

public class BacteriaAI : MonoBehaviour
{
    [Header("Chase Settings")]
    public float moveSpeed = 2f;
    public float detectionRange = 10f;
    public float retargetTime = 3f;
    public float attackDamage = 10f;

    [Header("Wander Settings")]
    public float wanderSpeed = 1.5f;
    public float wanderChangeInterval = 2f;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask bodyLayerMask = 3;

    private Transform target;
    private Rigidbody2D rb;
    private float targetTimer;
    private GameObject currentBodyTarget;
    private Vector2 wanderDirection;
    private float wanderTimer;
    private readonly Collider2D[] nearbyResults = new Collider2D[16];

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        PickNewWanderDirection();
        wanderTimer = wanderChangeInterval;
    }

    void Update()
    {
        DetectAndAssignBodyTarget();

        if (target == null)
        {
            Wander();
        }
        else
        {
            ChaseTarget();
        }
    }

    // Picks a new random direction for wandering movement
    void Wander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            PickNewWanderDirection();
            wanderTimer = wanderChangeInterval;
        }

        rb.linearVelocity = wanderDirection * wanderSpeed;
    }

    // Generates a random normalized direction vector for wandering
    void PickNewWanderDirection()
    {
        float angle = Random.Range(0f, 360f);
        float rad = angle * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }

    // Moves toward assigned body target, releases if timer expires or lost
    void ChaseTarget()
    {
        if (target == null) return;

        targetTimer -= Time.deltaTime;
        if (targetTimer <= 0f)
        {
            ReleaseCurrentTarget();
            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance < detectionRange)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            ReleaseCurrentTarget();
        }
    }

    // Looks for body parts in range to attack
    void DetectAndAssignBodyTarget()
    {
        if (target != null) return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = bodyLayerMask;

        int hitCount = Physics2D.OverlapCircle(transform.position, detectionRange,
            filter, nearbyResults);

        GameObject closestBody = null;
        float closestDist = detectionRange;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = nearbyResults[i];
            if (col != null && col.CompareTag("Body"))
            {
                float d = Vector2.Distance(transform.position, col.transform.position);
                if (d < closestDist)
                {
                    closestDist = d;
                    closestBody = col.gameObject;
                }
            }
        }

        if (closestBody != null)
        {
            currentBodyTarget = BacteriaManager.Instance.GetLeastTargetedBody();
            if (currentBodyTarget != null)
            {
                target = currentBodyTarget.transform;
                targetTimer = retargetTime;
            }
        }
    }

    // Releases current target from manager tracking
    void ReleaseCurrentTarget()
    {
        if (currentBodyTarget != null)
        {
            BacteriaManager.Instance.ReleaseTarget(currentBodyTarget);
            currentBodyTarget = null;
        }
        target = null;
    }

    void OnDestroy()
    {
        if (currentBodyTarget != null)
        {
            BacteriaManager.Instance.ReleaseTarget(currentBodyTarget);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Antibacterial"))
        {
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Body"))
        {
            // Attack logic here
        }
    }

    // Visualize detection range in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
