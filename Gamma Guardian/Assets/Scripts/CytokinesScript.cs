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


    private NavMeshAgent agent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<GameObject> allSpawns = GameObject.FindGameObjectsWithTag("Spawn").ToList();
        int randomIndex = Random.Range(0, allSpawns.Count);
        target = allSpawns[randomIndex].transform;
        pulseEffect = GetComponentInChildren<PulseEffect>();
        pulseEffect = GetComponentInChildren<PulseEffect>();
        if (pulseEffect != null)
            pulseEffect.SetPulseActive(false);
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        agent.SetDestination(target.position);

        if (Vector2.Distance(transform.position, target.position) < 6)
        {
            if (!deactivated)
                Instantiate(immuneCell, transform.position, Quaternion.identity);
            Instantiate(burst, transform.position, Quaternion.identity);
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
        }
    }
}
