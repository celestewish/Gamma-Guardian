using System.Collections;
using UnityEngine;

public class TutorialCytokine : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public float moveSpeed = 2f;
    public float stationaryTime = 3f; // Seconds to stay still before "vulnerable"

    private bool isVulnerable = false;
    private Vector3 wanderTarget;
    private TutorialManager tutorialManager;

    void Start()
    {
        tutorialManager = FindFirstObjectByType<TutorialManager>();
        StartCoroutine(WanderBehavior());
    }

    void Update()
    {
        if (isVulnerable)
        {
            transform.position = Vector2.MoveTowards(transform.position, wanderTarget, moveSpeed * 0.3f * Time.deltaTime);
        }
        else
        {
            // Normal wandering
            transform.position = Vector2.MoveTowards(transform.position, wanderTarget, moveSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, wanderTarget) < 0.1f)
            {
                PickNewTarget();
            }
        }
    }

    IEnumerator WanderBehavior()
    {
        while (!isVulnerable)
        {
            yield return new WaitForSeconds(stationaryTime);
            isVulnerable = true;
        }
    }

    void PickNewTarget()
    {
        wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * 5f;
    }

    public void Deactivate()
    {
        tutorialManager?.OnGammaCalmed(); // Signal tutorial progress
        // Spawn immune cell will go here
        Destroy(gameObject);
    }

    void OnMouseDown() // Medicine button taps this
    {
        if (isVulnerable)
        {
            Deactivate();
        }
    }
}
