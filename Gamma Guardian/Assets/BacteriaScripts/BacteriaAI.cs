using UnityEngine;

public class BacteriaAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float attackDamage = 10f;
    public float detectionRange = 10f;
    public float retargetTime = 5f;

    private Transform target;
    private Rigidbody2D rb;
    private float targetTimer;
    private GameObject currentBodyTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        FindLeastTargetedBody();
    }

    void Update()
    {
        if (target == null)
        {
            FindLeastTargetedBody();
            return;
        }

        targetTimer -= Time.deltaTime;
        if (targetTimer <= 0f)
        {
            BacteriaManager.Instance.ReleaseTarget(currentBodyTarget);
            FindLeastTargetedBody();
        }

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance < detectionRange)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void FindLeastTargetedBody()
    {
        currentBodyTarget = BacteriaManager.Instance.GetLeastTargetedBody();
        if (currentBodyTarget != null)
        {
            target = currentBodyTarget.transform;
            targetTimer = retargetTime;
        }
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
            // System for when attacking the body goes here
            BacteriaManager.Instance.ReleaseTarget(currentBodyTarget);
        }
    }
}

