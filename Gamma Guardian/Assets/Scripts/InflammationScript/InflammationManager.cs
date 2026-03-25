using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InflammationManager : MonoBehaviour
{
    public static InflammationManager Instance;
    public InflammationVignette InflammationVignette;
    public InflammationBar InflammationBar;
    public float currentInflammation = 0f;
    public float inflammationRatePerCell = 2f;
    public float decayRate = 0.5f;
    public float maxInflammation = 100f;

    private int totalAttachedCells = 0;
    private float attachedCellCount => totalAttachedCells;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (InflammationVignette == null)
        {
            GameObject vignetteObj = GameObject.Find("VignetteOverlay");
            InflammationVignette = vignetteObj?.GetComponent<InflammationVignette>();
            Debug.Log($"Vignette: {InflammationVignette}");  // Test
        }
        if (InflammationBar == null)
        {
            GameObject barObj = GameObject.Find("completionFill");
            InflammationBar = barObj?.GetComponent<InflammationBar>();
            Debug.Log($"Bar: {InflammationBar}");  // Test
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        currentInflammation += totalAttachedCells * inflammationRatePerCell * Time.deltaTime;
        if (totalAttachedCells == 0) currentInflammation -= decayRate * Time.deltaTime;
        currentInflammation = Mathf.Clamp(currentInflammation, 0f, maxInflammation);

        if (InflammationVignette != null)
            InflammationVignette.SetInflammation(currentInflammation / maxInflammation);
        if (InflammationBar != null)
            InflammationBar.SetInflammation(currentInflammation / maxInflammation);

        if (currentInflammation >= maxInflammation) { 
            Debug.Log("Game Over!");
            GameManager.Instance.gameLost = true;
            GameManager.Instance.EndLevel();
        }
    }

    public void AddAttachedCell() => totalAttachedCells++;
    public void RemoveAttachedCell() => totalAttachedCells = Mathf.Max(0, totalAttachedCells - 1);
}
