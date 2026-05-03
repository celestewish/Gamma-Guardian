using UnityEngine;

public class TutorialImmuneCell : MonoBehaviour
{
    [Header("Wander Settings")]
    public float wanderSpeed = 1.5f;
    public float wanderChangeInterval = 2f;

    private Rigidbody2D rb;
    private Vector2 wanderDirection;
    private float wanderTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        PickNewWanderDirection();
        wanderTimer = wanderChangeInterval;
    }

    void Update()
    {
        Wander();
    }

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

    void PickNewWanderDirection()
    {
        float angle = Random.Range(0f, 360f);
        float rad = angle * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }
}