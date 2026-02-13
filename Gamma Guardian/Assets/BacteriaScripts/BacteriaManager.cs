using UnityEngine;
using System.Collections.Generic;

public class BacteriaManager : MonoBehaviour
{
    public static BacteriaManager Instance;

    private Dictionary<GameObject, int> targetCounts = new Dictionary<GameObject, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetLeastTargetedBody()
    {
        GameObject[] bodies = GameObject.FindGameObjectsWithTag("Body");
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
