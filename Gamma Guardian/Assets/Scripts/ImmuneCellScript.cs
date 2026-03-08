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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        TryBacteria();
    }
    void TryBacteria()
    {
        List<GameObject> allBacteria = GameObject.FindGameObjectsWithTag("Bacteria")
            .OrderBy(bacteria => Vector2.Distance(transform.position, bacteria.transform.position))
            .ToList();
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
