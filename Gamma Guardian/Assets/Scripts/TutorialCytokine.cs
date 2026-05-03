using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TutorialCytokine : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float nearRadius = 1.5f;

    [Header("Effects")]
    public GameObject burst;
    public GameObject healEffect;
    public float inflammationReduction = 5f;
    public float pulseRange = 4f;

    [Header("Wander Settings")]
    public float wanderSpeed = 1.5f;
    public float wanderChangeInterval = 2f;
    private Vector2 wanderDirection;
    private float wanderTimer;

    private Rigidbody2D rb;
    public Transform player;
    private PulseEffect pulseEffect;
    public bool deactivated = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        PickNewWanderDirection();
        wanderTimer = wanderChangeInterval;
        pulseEffect = GetComponentInChildren<PulseEffect>();
        if (pulseEffect != null) pulseEffect.SetPulseActive(false);
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        Wander();
        // Pulse when player is nearby
        float distToPlayer = Vector2.Distance(transform.position, player.position);
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

    public void Deactivate()
    {
        Debug.Log("TutorialCytokine.Deactivate() called. Deactivated: " + deactivated);
        if (deactivated) return;
        if (transform.Find("Neutral") != null)
        {
            transform.Find("Neutral").gameObject.SetActive(true);
            pulseEffect?.SetPulseActive(false);
            if (healEffect != null) Instantiate(healEffect, transform.position, Quaternion.identity);
            deactivated = true;
            InflammationManager.Instance?.ReduceInflammation(inflammationReduction);

            // Notify TutorialManager
            TutorialManager tutManager = FindFirstObjectByType<TutorialManager>();
            Debug.Log("TutorialManager found: " + (tutManager != null));
            if (tutManager != null) tutManager.OnGammaCalmed();
            Destroy(gameObject, 2f);
        }
        if (transform.Find("Neutral") == null) Debug.Log("is inactive");
    }
}
