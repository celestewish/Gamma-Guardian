using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CytokinesScript : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float nearRadius = 1.5f;

    private Transform target;
    public GameObject immuneCell;
    private bool activated = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<GameObject> allSpawns = GameObject.FindGameObjectsWithTag("Spawn").ToList();
        int randomIndex = Random.Range(0, allSpawns.Count);
        target = allSpawns[randomIndex].transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, target.position) < nearRadius)
        {
            if (!activated)
                Instantiate(immuneCell, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
