using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Level State")]
    public bool levelRunning = false;
    public int bacteriaCount = 0;

    [Header("UI")]
    public UnityEngine.UI.Image completionBar;
    private InflammationManager inflammationManager;
    public float progress = 0f;

    [Header("Pause")]
    public bool isPaused = false;
    public GameObject pauseMenuUI;

    [Header("Timer")]
    public TextMeshProUGUI timerText;
    public float levelTimeLimit = 300f;
    private float timeRemaining;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartLevel();
    }
    void Update()
    {
        if (completionBar != null)
        {
            progress = inflammationManager.currentInflammation / 100f;
            completionBar.fillAmount =
                Mathf.Lerp(completionBar.fillAmount, progress, Time.deltaTime * 5f);
        }
    }

    public void StartLevel()
    {
        BacteriaAI[] bacteria = Object.FindObjectsByType<BacteriaAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        bacteriaCount = bacteria.Length;
        inflammationManager = InflammationManager.Instance;
        levelRunning = true;

        timeRemaining = levelTimeLimit;
        UpdateTimerUI();
        InvokeRepeating(nameof(TickTimer), 1f, 1f);

        Debug.Log($"Level started with {bacteriaCount} bacteria.");
    }

    public void OnBacteriaDestroyed()
    {
        if (!levelRunning) return;

        bacteriaCount = Mathf.Max(0, bacteriaCount - 1);
        Debug.Log($"Bacteria destroyed, remaining: {bacteriaCount}");

        if (bacteriaCount == 0)
        {
            EndLevel();
        }
    }

    private void EndLevel()
    {
        levelRunning = false;
        Debug.Log("Level complete! All bacteria destroyed.");
    }

    private void TickTimer()
    {
        if (!levelRunning || isPaused) return;

        timeRemaining -= 1f;
        UpdateTimerUI();

        if (timeRemaining <= 0)
        {
            EndLevelByTimeout();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void EndLevelByTimeout()
    {
        levelRunning = false;
        Debug.Log("Time's up! Level failed.");
        Time.timeScale = 0f;
    }
    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Debug.Log("Game Paused");
    }

    public void UnpauseGame()
    {
        if (!isPaused) return;
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Debug.Log("Game Unpaused");
    }

    public void TogglePause()
    {
        if (isPaused) UnpauseGame();
        else PauseGame();
    }

    public void GoHome()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }
}

