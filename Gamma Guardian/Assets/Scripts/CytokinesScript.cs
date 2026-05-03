using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CytokinesScript : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float nearRadius = 1.5f;

    private Transform target;
    public GameObject immuneCell;
    public bool deactivated = false;
    public GameObject burst;
    public GameObject healEffect;

    public Transform player;
    public float pulseRange = 4f;
    private PulseEffect pulseEffect;
    public float inflammationReduction = 5f;

    public bool isTutorialMode = false;
    public Transform tutorialTarget;


    private NavMeshAgent agent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError("NavMeshAgent component missing!");
        else if (!agent.isOnNavMesh)
            Debug.LogError("Agent is NOT on NavMesh! Position: " + transform.position);
        if (isTutorialMode)
        {
            target = tutorialTarget;
            return;
        }
        List<GameObject> allSpawns = GameObject.FindGameObjectsWithTag("Spawn").ToList();
        if (allSpawns.Count > 0)
        {
            int randomIndex = Random.Range(0, allSpawns.Count);
            target = allSpawns[randomIndex].transform;
        }
        else
        {
            Debug.LogWarning("CytokinesScript: No Spawn points found in scene.");
        }
        pulseEffect = GetComponentInChildren<PulseEffect>();
        pulseEffect = GetComponentInChildren<PulseEffect>();
        if (pulseEffect != null)
            pulseEffect.SetPulseActive(false);
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;
        Debug.Log(target);
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            agent.SetDestination(target.position);

        //transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        agent.SetDestination(target.position);

        if (Vector2.Distance(transform.position, target.position) < 6)
        {
            if (!deactivated)
                Instantiate(immuneCell, transform.position, Quaternion.identity);
            Instantiate(burst, transform.position, Quaternion.identity);
            gameObject.BroadcastMessage("RemoveMeFromInfo");
            Destroy(gameObject);
        }
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        bool shouldPulse = distToPlayer < pulseRange && distToPlayer > 2;  // Between pulseRange & nearRadius
        if (pulseEffect != null)
        {
            if (shouldPulse && !pulseEffect.isPulsing)
                pulseEffect.SetPulseActive(true);
            else if (!shouldPulse && pulseEffect.isPulsing)
                pulseEffect.SetPulseActive(false);
        }
    }
    public void Deactivate()
    {
        if (transform.Find("Neutral") != null)
        {
            transform.Find("Neutral").gameObject.SetActive(true);
            pulseEffect?.SetPulseActive(false);
            Instantiate(healEffect, transform.position, Quaternion.identity);
            deactivated = true;
            InflammationManager.Instance?.ReduceInflammation(inflammationReduction);
        }
    }
}
