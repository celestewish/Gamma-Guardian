using UnityEngine;
using UnityEngine.AI;

public class TutorialBacteria : MonoBehaviour
{

    [Header("Wander Settings")]
    public float wanderSpeed = 1.5f;
    public float wanderChangeInterval = 2f;

    [Header("Pulse Settings")]
    public float pulseRange = 6f;
    public float nearRadius = 2f;

    [Header("VFX")]
    public GameObject bacteriaPuffPrefab;

    public Transform player;
    private PulseEffect pulseEffect;
    private Rigidbody2D rb;
    private NavMeshAgent agent;
    private Vector2 wanderDirection;
    private float wanderTimer;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        PickNewWanderDirection();
        wanderTimer = wanderChangeInterval;

        pulseEffect = GetComponentInChildren<PulseEffect>();
        if (pulseEffect != null) pulseEffect.SetPulseActive(false);

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }
    }

    void Update()
    {
        if (player == null || isDead) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        Wander();

        // Pulse effect
        bool shouldPulse = distToPlayer < pulseRange && distToPlayer > nearRadius;
        if (pulseEffect != null)
        {
            if (shouldPulse && !pulseEffect.isPulsing) pulseEffect.SetPulseActive(true);
            else if (!shouldPulse && pulseEffect.isPulsing) pulseEffect.SetPulseActive(false);
        }
    }

    void Wander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            PickNewWanderDirection();
            wanderTimer = wanderChangeInterval;
        }
        if (rb != null)
            rb.linearVelocity = wanderDirection * wanderSpeed;
    }

    void PickNewWanderDirection()
    {
        float angle = Random.Range(0f, 360f);
        float rad = angle * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (bacteriaPuffPrefab != null)
            Instantiate(bacteriaPuffPrefab, transform.position, transform.rotation);

        if (player != null)
            player.gameObject.SendMessage("PlayBactDeath", SendMessageOptions.DontRequireReceiver);

        // Notify TutorialManager
        TutorialManager tutManager = Object.FindFirstObjectByType<TutorialManager>();
        if (tutManager != null) tutManager.OnBacteriaDefeated();

        Destroy(gameObject);
    }
}
