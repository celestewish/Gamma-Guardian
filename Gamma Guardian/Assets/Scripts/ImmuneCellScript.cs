using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ImmuneCellScript : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float nearRadius = 1.5f;
    public float spawnTime = 3f;
    public float ICCheckRadius = 2f;

    public GameObject cytokines;
    private Transform target;
    private float timeNearTarget = 0f;

    [Header("Wander Settings")]
    public float wanderSpeed = 1.5f;
    public float wanderChangeInterval = 2f;

    [Header("Sprite States")]
    public Sprite normalSprite;
    public Sprite distressedSprite;
    public Sprite criticalSprite;
    private SpriteRenderer spriteRenderer;


    private Rigidbody2D rb;
    private Vector2 wanderDirection;
    private float wanderTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        PickNewWanderDirection();
        wanderTimer = wanderChangeInterval;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSpriteBasedOnInflammation();
        TryBacteria();
    }

    void UpdateSpriteBasedOnInflammation()
    {
        if (spriteRenderer == null || InflammationManager.Instance == null) return;

        float inflammationPct = InflammationManager.Instance.currentInflammation / InflammationManager.Instance.maxInflammation;

        if (inflammationPct > 0.5f && criticalSprite != null)
        {
            spriteRenderer.sprite = criticalSprite;
        }
        else if (inflammationPct > 0.25f && distressedSprite != null)
        {
            spriteRenderer.sprite = distressedSprite;
        }
        else if (normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }
    }

    void TryBacteria()
    {
        List<GameObject> allBacteria = GameObject.FindGameObjectsWithTag("Bacteria")
            .OrderBy(bacteria => Vector2.Distance(transform.position, bacteria.transform.position))
            .ToList();
        if (allBacteria.Count == 0 )
        {
            Wander();
            return;
        }
        foreach (GameObject bacteriaObj in allBacteria)
        {
            if (!HasOtherImmuneCellNearby(bacteriaObj.transform))
            {
                target = bacteriaObj.transform;
                MoveTowards(target.position);

                if (Vector2.Distance(transform.position, target.position) <= nearRadius)
                {
                    MoveTowards(target.position);
                    timeNearTarget += Time.deltaTime;
                    if (timeNearTarget >= spawnTime)
                    {
                        Instantiate(cytokines, transform.position, Quaternion.identity);
                        timeNearTarget = 0f;
                    }
                }
                else
                    timeNearTarget = 0f;
                return;
            }
        }
        GoToBody();
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

    // Generates a random normalized direction vector for wandering
    void PickNewWanderDirection()
    {
        float angle = Random.Range(0f, 360f);
        float rad = angle * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }
    void GoToBody()
    {
        target = GameObject.FindGameObjectsWithTag("Body")
            .OrderBy(body => Vector2.Distance(transform.position, body.transform.position))
            .First().transform;
        MoveTowards(target.position);
    }
    bool HasOtherImmuneCellNearby(Transform BacteriaTransform)
    {
        Transform closestImmuneCell = GameObject.FindGameObjectsWithTag("ImmuneCell")
            .OrderBy(ImmuneCell => Vector2.Distance(BacteriaTransform.position, ImmuneCell.transform.position))
            .First().transform;
        if (Vector2.Distance(BacteriaTransform.position, closestImmuneCell.transform.position) <= ICCheckRadius && closestImmuneCell.transform != this.transform)
            return true;
        else return false;
    }
    void MoveTowards(Vector2 t)
    {
        if (Vector2.Distance(transform.position, t) >= 1f)
            transform.position = Vector2.MoveTowards(transform.position, t, moveSpeed * Time.deltaTime);
    }
}
