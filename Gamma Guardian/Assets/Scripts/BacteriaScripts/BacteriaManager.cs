using UnityEngine;
using System.Collections.Generic;

public class BacteriaManager : MonoBehaviour
{
    public static BacteriaManager Instance;

    private GameObject[] bodies;

    private Dictionary<GameObject, int> targetCounts = new Dictionary<GameObject, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            bodies = GameObject.FindGameObjectsWithTag("Body");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Looks for body part that has the least bacteria on it
    public GameObject GetLeastTargetedBody()
    {
        GameObject bestTarget = null;
        int minCount = int.MaxValue;

        foreach (GameObject body in bodies)
        {
            targetCounts.TryGetValue(body, out int count);
            if (count < minCount)
            {
                minCount = count;
                bestTarget = body;
            }
        }

        if (bestTarget != null)
        {
            targetCounts[bestTarget] = targetCounts.GetValueOrDefault(bestTarget, 0) + 1;
        }

        return bestTarget;
    }
    //Removes the body part from the targeted dict
    public void ReleaseTarget(GameObject body)
    {
        if (targetCounts.ContainsKey(body))
        {
            targetCounts[body]--;
            if (targetCounts[body] <= 0)
            {
                targetCounts.Remove(body);
            }
        }
    }
}
