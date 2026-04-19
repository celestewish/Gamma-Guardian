using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

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

    [Header("Swarmer Settings")]
    public float swarmDetectRange = 6f;
    public float swarmMoveSpeed = 3.5f;
    public float swarmOffsetRadius = 1.5f;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask bodyLayerMask = 3;

    [Header("VFX")]
    public GameObject bacteriaPuffPrefab;

    private bool isSwarmer;
    private bool swarmTriggered;
    private Transform target;
    private Rigidbody2D rb;
    private float targetTimer;
    private GameObject currentBodyTarget;
    private Vector2 wanderDirection;
    private float wanderTimer;
    private readonly Collider2D[] nearbyResults = new Collider2D[16];
    private bool isDead = false;

    public Transform player;
    public float pulseRange = 6f;
    private PulseEffect pulseEffect;
    public float nearRadius = 2f;

    private NavMeshAgent agent;
    
    public GameObject textDisplay;
    private bool displayOn = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        PickNewWanderDirection();
        wanderTimer = wanderChangeInterval;
        pulseEffect = GetComponentInChildren<PulseEffect>();
        if (pulseEffect != null)
            pulseEffect.SetPulseActive(false);
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        isSwarmer = SceneManager.GetActiveScene().name == "Level2";
        
        textDisplay.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        if (isSwarmer && PlayerInSwarmRange())
        {
            swarmTriggered = true;
            SwarmPlayer();
        }
        else
        {
            swarmTriggered = false;
            DetectAndAssignBodyTarget();

            if (target == null)
                Wander();
            else
                ChaseTarget();
        }
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        bool shouldPulse = distToPlayer < pulseRange && distToPlayer > nearRadius;  // Between pulseRange & nearRadius
        if (pulseEffect != null)
        {
            if (shouldPulse && !pulseEffect.isPulsing)
                pulseEffect.SetPulseActive(true);
            else if (!shouldPulse && pulseEffect.isPulsing)
                pulseEffect.SetPulseActive(false);
        }
    }

    bool PlayerInSwarmRange()
    {
        return Vector2.Distance(transform.position, player.position) <= swarmDetectRange;
    }

    void SwarmPlayer()
    {
        ReleaseCurrentTarget();

        Vector2 offset = ((Vector2)transform.position - (Vector2)player.position).normalized;
        if (offset == Vector2.zero)
            offset = Random.insideUnitCircle.normalized;

        Vector2 swarmPoint = (Vector2)player.position + offset * swarmOffsetRadius;

        if (agent != null)
        {
            agent.speed = swarmMoveSpeed;
            agent.SetDestination(swarmPoint);
        }
        else if (rb != null)
        {
            Vector2 direction = (swarmPoint - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * swarmMoveSpeed;
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
            //Vector2 direction = (target.position - transform.position).normalized;
            //rb.linearVelocity = direction * moveSpeed;

            agent.SetDestination(target.position);
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

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBacteriaDestroyed();
        }
        BacteriaManager.Instance.OnBacteriaDied();
        if (bacteriaPuffPrefab != null)
            Instantiate(bacteriaPuffPrefab, transform.position, transform.rotation);
        player.gameObject.SendMessage("PlayBactDeath");


        Destroy(gameObject);
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
        if (collision.gameObject.CompareTag("ImmuneCell"))
        {
            //Die();
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

    void DisplayText()
    {
        //
        displayOn = !displayOn;
        textDisplay.SetActive(displayOn);
    }
}
