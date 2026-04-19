using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
//using TMPro;
//using UnityEngine.UI;


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

    [Header("Evasive Settings")]
    private bool isEvasive;
    private bool isEvading;
    public float evadeTriggerRange = 4f;
    public float evadeSpeed = 4.5f;
    public float evadeDuration = 1.25f;
    private float evadeTimer;

    private bool isAggressive;
    private bool isEnraged;

    [Header("Aggressive Settings")]
    public float aggressionSpeed = 4f;
    public float aggressionDuration = 2.5f;
    public float aggressionStopDistance = 1.2f;
    private float aggressionTimer;

    private enum BehaviorBias
    {
        Swarmer,
        Evasive,
        Aggressive
    }

    private BehaviorBias behaviorBias;

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        PickNewWanderDirection();
        wanderTimer = wanderChangeInterval;

        pulseEffect = GetComponentInChildren<PulseEffect>();
        if (pulseEffect != null)
            pulseEffect.SetPulseActive(false);

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        string sceneName = SceneManager.GetActiveScene().name;

        isSwarmer = sceneName == "Level2" || sceneName == "Level5";
        isEvasive = sceneName == "Level3" || sceneName == "Level5";
        isAggressive = sceneName == "Level4" || sceneName == "Level5";

        if (sceneName == "Level5")
        {
            AssignBehaviorBias();
            ApplyBehaviorBias();
        }

        if (textDisplay != null)
            textDisplay.SetActive(false);
    }

    void Update()
    {
        if (player == null || isDead) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (isEnraged)
        {
            HandleAggression();
        }
        else if (isEvasive && distToPlayer <= evadeTriggerRange)
        {
            HandleEvasiveBehavior(distToPlayer);
        }
        else if (isSwarmer && distToPlayer <= swarmDetectRange)
        {
            SwarmPlayer();
        }
        else
        {
            DetectAndAssignBodyTarget();

            if (target == null)
                Wander();
            else
                ChaseTarget();
        }
        bool shouldPulse = distToPlayer < pulseRange && distToPlayer > nearRadius;  // Between pulseRange & nearRadius
        if (pulseEffect != null)
        {
            if (shouldPulse && !pulseEffect.isPulsing)
                pulseEffect.SetPulseActive(true);
            else if (!shouldPulse && pulseEffect.isPulsing)
                pulseEffect.SetPulseActive(false);
        }
    }

    void AssignBehaviorBias()
    {
        int roll = Random.Range(0, 100);

        if (roll < 40)
            behaviorBias = BehaviorBias.Swarmer;
        else if (roll < 75)
            behaviorBias = BehaviorBias.Evasive;
        else
            behaviorBias = BehaviorBias.Aggressive;
    }

    void ApplyBehaviorBias()
    {
        switch (behaviorBias)
        {
            case BehaviorBias.Swarmer:
                swarmDetectRange += 1.5f;
                swarmMoveSpeed += 0.5f;
                evadeTriggerRange -= 0.5f;
                aggressionDuration -= 0.5f;
                break;

            case BehaviorBias.Evasive:
                evadeTriggerRange += 1.5f;
                evadeDuration += 0.5f;
                evadeSpeed += 0.5f;
                swarmDetectRange -= 0.5f;
                break;

            case BehaviorBias.Aggressive:
                aggressionDuration += 1f;
                aggressionSpeed += 0.75f;
                aggressionStopDistance += 0.25f;
                evadeDuration -= 0.25f;
                break;
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

    void HandleEvasiveBehavior(float distToPlayer)
    {
        if (distToPlayer <= evadeTriggerRange)
        {
            isEvading = true;
            evadeTimer = evadeDuration;
        }

        if (isEvading)
        {
            ReleaseCurrentTarget();

            evadeTimer -= Time.deltaTime;

            Vector2 fleeDirection = ((Vector2)transform.position - (Vector2)player.position).normalized;
            if (fleeDirection == Vector2.zero)
                fleeDirection = Random.insideUnitCircle.normalized;

            Vector2 fleeTarget = (Vector2)transform.position + fleeDirection * 3f;

            if (agent != null)
            {
                agent.speed = evadeSpeed;
                agent.SetDestination(fleeTarget);
            }
            else if (rb != null)
            {
                rb.linearVelocity = fleeDirection * evadeSpeed;
            }

            if (evadeTimer <= 0f)
                isEvading = false;
        }
        else
        {
            DetectAndAssignBodyTarget();

            if (target == null)
                Wander();
            else
                ChaseTarget();
        }
    }

    public void TriggerAggression()
    {
        if (!isAggressive || isDead) return;

        isEnraged = true;
        aggressionTimer = aggressionDuration;
        ReleaseCurrentTarget();
    }

    void HandleAggression()
    {
        aggressionTimer -= Time.deltaTime;

        ReleaseCurrentTarget();

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer > aggressionStopDistance)
        {
            if (agent != null)
            {
                agent.speed = aggressionSpeed;
                agent.SetDestination(player.position);
            }
            else if (rb != null)
            {
                Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
                rb.linearVelocity = dir * aggressionSpeed;
            }
        }
        else
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        if (aggressionTimer <= 0f)
        {
            isEnraged = false;
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
}
