using UnityEngine;
using System.Collections.Generic;
using System; // For Action

public class BacteriaManager : MonoBehaviour
{
    public static BacteriaManager Instance;

    private GameObject[] bodies;
    private Dictionary<GameObject, int> targetCounts = new Dictionary<GameObject, int>();
    private int totalAliveBacteria = 0;

    public int TotalAliveBacteria => totalAliveBacteria;
    public bool AreBacteriaActive => totalAliveBacteria > 0;

    public static event Action OnAllBacteriaDead;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            bodies = GameObject.FindGameObjectsWithTag("Body");
            CountInitialBacteria();
        }
        else
        {
            Destroy(gameObject);
        }
    }

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

    public void OnBacteriaDied()
    {
        totalAliveBacteria--;
        GameManager.Instance.RegisterBacteriaKill();
        if (totalAliveBacteria <= 0)
        {
            totalAliveBacteria = 0;
            OnAllBacteriaDead?.Invoke();
        }
    }
    public void CountInitialBacteria()
    {
        totalAliveBacteria = GameObject.FindGameObjectsWithTag("Bacteria").Length;
    }
}

