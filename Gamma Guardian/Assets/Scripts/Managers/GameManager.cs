using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Level State")]
    public bool levelRunning = false;
    public int bacteriaCount = 0;
    public bool gameWon = false;
    public bool gameLost = false;

    [Header("UI")]
    public Image completionBar;
    private InflammationManager inflammationManager;
    public float progress = 0f;
    public FadeController fadeController;

    [Header("Pause")]
    public bool isPaused = false;
    public GameObject pauseMenuUI;

    [Header("Timer")]
    public TextMeshProUGUI timerText;
    public float levelTimeLimit = 300f;
    private float timeRemaining;

    [Header("Combos")]
    public TextMeshProUGUI comboText; // UI Text for "3+" pop-up (child of Canvas)
    public float comboWindow = 3f;
    public float comboReductionBase = 0.05f; // 5% per combo level, tune

    private int currentCombo = 0;
    private float lastKillTime;
    private float comboTimer;
    private string comboType = "Bacteria";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if ((scene.name == "Level" || scene.name == "LaurenLevelScene") && !levelRunning)
        {
            if (fadeController == null) fadeController = GameObject.Find("FadeCanvas")?.GetComponent<FadeController>();
            StartLevel();
        }
    }

    void OnDestroy()
    {
        // Clean up:
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    void Start()
    {
        StartLevel();
    }
    void Update()
    {
        if (completionBar != null && inflammationManager != null)
        {
            float normalizedInflam = inflammationManager.currentInflammation / inflammationManager.maxInflammation;
            completionBar.fillAmount = Mathf.Lerp(completionBar.fillAmount, normalizedInflam, Time.deltaTime * 5f);
        }

        comboTimer += Time.deltaTime;
        if (comboTimer > comboWindow)
        {
            currentCombo = 0;
            comboTimer = 0f;
        }
    }

    public void StartLevel()
    {
        if (completionBar == null)
        {
            GameObject barObj = GameObject.Find("completionFill");
            completionBar = barObj?.GetComponent<UnityEngine.UI.Image>();
            Debug.Log($"completionBar: {completionBar}");
        }
        if (timerText == null)
        {
            GameObject timerObj = GameObject.Find("timerText");
            timerText = timerObj?.GetComponent<TextMeshProUGUI>();
        }
        if (pauseMenuUI == null) pauseMenuUI = GameObject.Find("PauseCanvas");
        if (comboText == null) comboText = GameObject.Find("ComboText")?.GetComponent<TextMeshProUGUI>();


        BacteriaAI[] bacteria = Object.FindObjectsByType<BacteriaAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        bacteriaCount = bacteria.Length;
        inflammationManager = InflammationManager.Instance;
        if (inflammationManager == null) Debug.LogError("InflammationManager.Instance is null!");
        levelRunning = true;

        timeRemaining = levelTimeLimit;
        UpdateTimerUI();
        InvokeRepeating(nameof(TickTimer), 1f, 1f);
        currentCombo = 0;

        gameWon = false;
        gameLost = false;
        fadeController.FadeOut();

        Debug.Log($"Level started with {bacteriaCount} bacteria.");
    }

    public void OnBacteriaDestroyed()
    {
        if (!levelRunning) return;

        bacteriaCount = Mathf.Max(0, bacteriaCount - 1);
        Debug.Log($"Bacteria destroyed, remaining: {bacteriaCount}");

        if (bacteriaCount == 0)
        {
            gameWon = true;
            EndLevel();
        }
    }

    public void EndLevel()
    {
        levelRunning = false;
        CancelInvoke(nameof(TickTimer)); // Stop timer

        if (gameWon)
        {
            StartCoroutine(LoadLevelCoroutine("Ending"));
        }
        else if (gameLost)
        {
            StartCoroutine(LoadLevelCoroutine(SceneManager.GetActiveScene().name));
        }
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

    public void RegisterBacteriaKill()
    {
        comboTimer = 0f;
        lastKillTime = Time.time;

        currentCombo++;
        if (currentCombo >= 3)
        {
            TriggerComboBonus();
        }
    }

    private void TriggerComboBonus()
    {
        // Reduce inflammation
        float reduction = comboReductionBase * currentCombo;
        completionBar.fillAmount = Mathf.Max(0f, completionBar.fillAmount - reduction);

        // Sound
        // GetComponent<AudioSource>().PlayOneShot(comboSound);

        // Pop-up
        if (comboText != null)
        {
            Debug.Log("This has run");
            comboText.gameObject.SetActive(true);
            comboText.text = $"{currentCombo}+";

            Color originalColor = comboText.color;
            originalColor.a = 1f;
            comboText.color = originalColor;
            comboText.transform.localScale = Vector3.zero;

            comboText.transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack);
            DOVirtual.DelayedCall(1.5f, () => {
                comboText.DOFade(0, 0.3f).OnComplete(() => {
                    comboText.gameObject.SetActive(false);
                });
            });
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
        EndLevel();
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

    private IEnumerator LoadLevelCoroutine(string levelName)
    {
        fadeController.FadeIn();
        yield return new WaitForSecondsRealtime(2f);
        SceneManager.LoadScene(levelName, LoadSceneMode.Single);
    }
}

