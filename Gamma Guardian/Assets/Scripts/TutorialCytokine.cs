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
    public GameObject bacteria;
    public GameObject healEffect;

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
        if (transform.Find("Neutral") != null)
        {
            transform.Find("Neutral").gameObject.SetActive(true);
        }
        Instantiate(healEffect, transform.position, Quaternion.identity);
        tutorialManager?.OnGammaCalmed(); // Signal tutorial progress
        Destroy(gameObject, 1f);
        bacteria.SetActive(true);
    }
}
