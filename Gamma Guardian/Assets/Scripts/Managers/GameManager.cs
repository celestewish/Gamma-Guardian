using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.UI;
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

    [Header("Progression")]
    public int thisLevelIndex = 1;

    [Header("Gameplay Flow")]
    public bool timerStarted = false;
    public bool introDialogueFinished = false;
    private bool waitingForEndDialogue = false;
    private bool waitingForFailDialogue = false;

    [Header("UI")]
    public Image completionBar;
    private InflammationManager inflammationManager;
    public float progress = 0f;
    public FadeController fadeController;
    private GameObject uiCanvas;

    [Header("Pause")]
    public bool isPaused = false;
    public GameObject pauseMenuUI;
    public Button pauseButton;
    public Button resumeButton;
    public Button homeButton;
    public Button modeButton;
    private bool infoMode = false;

    [Header("Timer")]
    public TextMeshProUGUI timerText;
    public float levelTimeLimit = 300f;
    private float timeRemaining;

    [Header("Combos")]
    public TextMeshProUGUI comboText;
    public float comboWindow = 3f;
    public float comboReductionBase = 0.05f;

    [Header("Scenes")]
    public string mainMenuScene = "MainMenu";

    [Header("Dialogue")]
    public DialogueManager dialogueManager;
    #region Dialogue
    private string[] introDialogue =
    {
        "Welcome to the first vessel Guardian Explorer",
        "The infection is swarming this part of the body.",
        "Take out all the bacteria to save the patient!",
        "Make sure to take out cytokines too to slow infection."
    };
    public string[] endDialogue = {
        "Nice work Guardian!",
        "Our job's not over yet!",
        "There are more regions we need to tackle.",
        "Onwards!"
    };
    public string[] failDialogue = {
    "Don't give up, Guardian!",
    "That infection was tough, but you can beat it.",
    "Here's a tip. Prioritize the bacteria, but make sure to calm cytokines when flying by them!",
    "Take a breath and try again!"
};
    #endregion

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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sname = scene.name;

        if (sname == mainMenuScene)
        {
            ResetGameState();
            return;
        }

        bool isGameplayLevel = false;

        if (ProgressionManager.Instance != null)
            isGameplayLevel = ProgressionManager.Instance.IsGameplayScene(sname);
        else
            isGameplayLevel = sname.StartsWith("Level");

        if (isGameplayLevel && !levelRunning)
            StartLevel();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        if (levelRunning) return;

        uiCanvas = GameObject.Find("UserInterface");
        if (uiCanvas != null)
        {
            completionBar = GameObject.Find("completionFill")?.GetComponent<Image>();
            timerText = GameObject.Find("timerText")?.GetComponent<TextMeshProUGUI>();
            pauseMenuUI = GameObject.Find("PauseCanvas");
            comboText = GameObject.Find("ComboText")?.GetComponent<TextMeshProUGUI>();

            pauseButton = GameObject.Find("pause")?.GetComponent<Button>();
            resumeButton = GameObject.Find("Play")?.GetComponent<Button>();
            homeButton = GameObject.Find("Home")?.GetComponent<Button>();
            modeButton = GameObject.Find("mode")?.GetComponent<Button>();

            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveAllListeners();
                pauseButton.onClick.AddListener(TogglePause);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(TogglePause);
            }

            if (homeButton != null)
            {
                homeButton.onClick.RemoveAllListeners();
                homeButton.onClick.AddListener(GoHome);
            }

            if (modeButton != null)
            {
                modeButton.onClick.RemoveAllListeners();
                modeButton.onClick.AddListener(SwitchMode);
            }
        }

        BacteriaAI[] bacteria = Object.FindObjectsByType<BacteriaAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        bacteriaCount = bacteria.Length;

        inflammationManager = InflammationManager.Instance;
        levelRunning = true;
        gameWon = false;
        gameLost = false;
        isPaused = false;

        timeRemaining = levelTimeLimit;
        timerStarted = false;
        introDialogueFinished = false;
        currentCombo = 0;

        UpdateTimerUI();

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (fadeController == null)
            fadeController = GameObject.Find("FadeCanvas")?.GetComponent<FadeController>();

        if (fadeController != null)
            fadeController.FadeOut();

        dialogueManager = FindFirstObjectByType<DialogueManager>();

        timerStarted = false;
        introDialogueFinished = false;

        if (dialogueManager != null)
        {
            dialogueManager.onDialogueEnd.RemoveListener(BeginGameplay);
            dialogueManager.onDialogueEnd.RemoveListener(HandleEndDialogueFinished);
            dialogueManager.onDialogueEnd.AddListener(BeginGameplay);

            dialogueManager.SetDialogueLines(introDialogue);
            dialogueManager.StartDialogue();
        }
        else
        {
            BeginGameplay();
        }

        Debug.Log($"Level started: {bacteriaCount} bacteria");
    }
    public void BeginGameplay()
    {
        if (timerStarted) return;

        timerStarted = true;
        introDialogueFinished = true;
        InvokeRepeating(nameof(TickTimer), 1f, 1f);

        Debug.Log("Gameplay started after dialogue.");
    }

    public void StartEndDialogue()
    {
        levelRunning = false;
        timerStarted = false;
        CancelInvoke(nameof(TickTimer));

        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();

        if (dialogueManager != null)
        {
            waitingForEndDialogue = true;

            dialogueManager.onDialogueEnd.RemoveListener(BeginGameplay);
            dialogueManager.onDialogueEnd.RemoveListener(HandleEndDialogueFinished);
            dialogueManager.onDialogueEnd.AddListener(HandleEndDialogueFinished);

            dialogueManager.SetDialogueLines(endDialogue);
            dialogueManager.StartDialogue();
        }
        else
        {
            HandleEndDialogueFinished();
        }
    }

    private void HandleEndDialogueFinished()
    {
        if (!waitingForEndDialogue) return;

        waitingForEndDialogue = false;

        if (dialogueManager != null)
            dialogueManager.onDialogueEnd.RemoveListener(HandleEndDialogueFinished);

        if (ProgressionManager.Instance != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            int completedLevelIndex = ProgressionManager.Instance.GetLevelIndexFromScene(currentSceneName);

            if (completedLevelIndex > 0)
                ProgressionManager.Instance.MarkLevelCompleted(completedLevelIndex);
        }

        GoHome();
    }

    public void ResetGameState()
    {
        levelRunning = false;
        bacteriaCount = 0;
        gameWon = false;
        gameLost = false;
        isPaused = false;
        timerStarted = false;
        introDialogueFinished = false;
        waitingForEndDialogue = false;
        waitingForFailDialogue = false;

        CancelInvoke(nameof(TickTimer));
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (completionBar != null) completionBar.fillAmount = 0f;
        if (dialogueManager != null)
        {
            dialogueManager.onDialogueEnd.RemoveListener(BeginGameplay);
            dialogueManager.onDialogueEnd.RemoveListener(HandleEndDialogueFinished);
            dialogueManager.onDialogueEnd.RemoveListener(HandleFailDialogueFinished);
        }

        Debug.Log("GameManager RESET for main menu");
    }

    public void OnBacteriaDestroyed()
    {
        if (!levelRunning) return;

        bacteriaCount = Mathf.Max(0, bacteriaCount - 1);
        Debug.Log($"Bacteria destroyed, remaining: {bacteriaCount}");

        if (bacteriaCount == 0)
        {
            gameWon = true;
            StartEndDialogue();
        }
    }

    public void EndLevel()
    {
        levelRunning = false;
        timerStarted = false;
        CancelInvoke(nameof(TickTimer));
    }

    private void TickTimer()
    {
        if (!levelRunning || isPaused || !timerStarted) return;

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
        if (!levelRunning) return;

        gameLost = true;
        levelRunning = false;
        timerStarted = false;
        CancelInvoke(nameof(TickTimer));

        Debug.Log("Time's up! Level failed.");
        StartFailDialogue();
    }
    public void StartFailDialogue()
    {
        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();

        if (dialogueManager != null)
        {
            waitingForFailDialogue = true;

            dialogueManager.onDialogueEnd.RemoveListener(BeginGameplay);
            dialogueManager.onDialogueEnd.RemoveListener(HandleEndDialogueFinished);
            dialogueManager.onDialogueEnd.RemoveListener(HandleFailDialogueFinished);
            dialogueManager.onDialogueEnd.AddListener(HandleFailDialogueFinished);

            dialogueManager.SetDialogueLines(failDialogue);
            dialogueManager.StartDialogue();
        }
        else
        {
            HandleFailDialogueFinished();
        }
    }
    private void HandleFailDialogueFinished()
    {
        if (!waitingForFailDialogue) return;

        waitingForFailDialogue = false;

        if (dialogueManager != null)
            dialogueManager.onDialogueEnd.RemoveListener(HandleFailDialogueFinished);

        RestartLevel();
    }
    public void RestartLevel()
    {
        ResetGameState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        ResetGameState();
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void SwitchMode()
    {
        Debug.Log("mode switched");
        foreach(BacteriaAI ba in Object.FindObjectsByType<BacteriaAI>(FindObjectsSortMode.None))
        {
            ba.gameObject.SendMessage("DisplayText");
        }
    }

    private IEnumerator fadeScene()
    {
        fadeController.FadeIn();
        yield return new WaitForSeconds(5f);
    }
}

