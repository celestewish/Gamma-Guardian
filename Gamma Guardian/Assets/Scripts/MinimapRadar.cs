using UnityEngine;
using System.Collections.Generic;

public class MinimapRadar : MonoBehaviour
{
    [Header("Dot Prefabs (World Space, MinimapOnly layer)")]
    public GameObject bacteriaDotPrefab;
    public GameObject cytokineDotPrefab;
    public GameObject immuneCellDotPrefab;
    public GameObject playerDotPrefab;

    [Header("Tracking")]
    public Transform player;

    [Header("Dot Height")]
    [SerializeField] private float dotZ = 0f; // Z position for all dots

    private GameObject playerDotInstance;

    // Tracked pairs: world object → minimap dot
    private Dictionary<Transform, GameObject> bacteriaTracked = new Dictionary<Transform, GameObject>();
    private Dictionary<Transform, GameObject> cytokineTracked = new Dictionary<Transform, GameObject>();
    private Dictionary<Transform, GameObject> immuneTracked = new Dictionary<Transform, GameObject>();

    private int minimapLayer;

    void Start()
    {
        minimapLayer = LayerMask.NameToLayer("MinimapOnly");

        if (playerDotPrefab != null && player != null)
        {
            playerDotInstance = Instantiate(playerDotPrefab);
            SetLayerRecursively(playerDotInstance, minimapLayer);
        }
    }

    void Update()
    {
        TrackObjects<BacteriaAI>(bacteriaTracked, bacteriaDotPrefab);
        TrackObjects<TutorialBacteria>(bacteriaTracked, bacteriaDotPrefab);
        TrackObjects<CytokinesScript>(cytokineTracked, cytokineDotPrefab);
        TrackObjects<TutorialCytokine>(cytokineTracked, cytokineDotPrefab);
        TrackObjects<ImmuneCellScript>(immuneTracked, immuneCellDotPrefab);

        // Move player dot
        if (playerDotInstance != null && player != null)
            playerDotInstance.transform.position = new Vector3(player.position.x, player.position.y, dotZ);

        // Clean up destroyed tracked objects
        CleanDestroyed(bacteriaTracked);
        CleanDestroyed(cytokineTracked);
        CleanDestroyed(immuneTracked);
    }

    private void TrackObjects<T>(Dictionary<Transform, GameObject> dict, GameObject prefab) where T : MonoBehaviour
    {
        T[] found = Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var obj in found)
        {
            if (dict.ContainsKey(obj.transform)) continue;

            GameObject dot = Instantiate(prefab);
            SetLayerRecursively(dot, minimapLayer);
            dot.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, dotZ);
            dict[obj.transform] = dot;
        }

        // Update positions
        foreach (var kvp in dict)
        {
            if (kvp.Key != null && kvp.Value != null)
                kvp.Value.transform.position = new Vector3(kvp.Key.position.x, kvp.Key.position.y, dotZ);
        }
    }

    private void CleanDestroyed(Dictionary<Transform, GameObject> dict)
    {
        List<Transform> toRemove = new List<Transform>();
        foreach (var kvp in dict)
        {
            if (kvp.Key == null)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove)
            dict.Remove(key);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}