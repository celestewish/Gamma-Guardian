using DG.Tweening;
using System.Collections;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Variables
    public event System.Action onGameplayBegin;
    public event System.Action onGameplayEnd;
    public static GameManager Instance;

    [Header("Level State")]
    public bool levelRunning = false;
    public int bacteriaCount = 0;
    public bool gameWon = false;
    public bool gameLost = false;

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
    [Header("Level 1 Dialogue")]
    [SerializeField]
    private string[] level1IntroDialogue =
{
    "Welcome to the first vessel Guardian Explorer",
    "The infection is swarming this part of the body.",
    "<b>Take out all the bacteria to save the patient!<b>"
};

    [SerializeField]
    private string[] level1EndDialogue =
    {
    "Nice work Guardian!",
    "Our job's not over yet!",
    "There are more regions we need to tackle.",
    "Onwards!"
};

    [Header("Level 2 Dialogue")]
    [SerializeField]
    private string[] level2IntroDialogue =
    {
    "We've made it deeper into the body, Guardian Explorer.",
    "This region is more dangerous than the last.",
    "Clear the bacteria here and keep the inflammation under control."
};

    [SerializeField]
    private string[] level2EndDialogue =
    {
    "Excellent work, Guardian!",
    "This region is safe for now.",
    "But the infection is still spreading elsewhere.",
    "Let's keep going!"
};

    [Header("Fail Dialogue")]
    [SerializeField]
    private string[] failDialogue =
    {
    "Don't give up, Guardian!",
    "That infection was tough, but you can beat it.",
    "Here's a tip. <b>Clear bacteria in an area before moving on.<b>",
    "Take a breath and try again!"
};
    [Header("Level 3 Dialogue")]
    [SerializeField]
    private string[] level3IntroDialogue =
{
    "Guardian, this next region is much larger than the others.",
    "The infection has spread across two separate paths.",
    "You will need to explore both routes before this area can be fully cleared.",
    "To help with this, you can now drag bacteria and cytokines along with you.",
    "When either are in range, you can press the new tether button and tie the enemies to your ship.",
    "Hopefully this new ability will come in handy.",
    "Stay alert, cover the whole level, and don't leave either path unchecked."
};

    [SerializeField]
    private string[] level3EndDialogue =
    {
    "Excellent work, Guardian!",
    "You cleared both paths and secured this entire region.",
    "That was a large area, but you handled it well.",
    "Let's keep moving and finish driving back the infection."
};
    [Header("Level 4 Dialogue")]
    [SerializeField]
    private string[] level4IntroDialogue =
    {
    "Guardian Explorer, there is a new option for you.",
    "Going forward you are now able to dash.",
    "To do this, press the new dash button while holding in a direction.",
    "Push forward, good luck!"
};

    [SerializeField]
    private string[] level4EndDialogue =
    {
    "Nicely done, Guardian!",
    "Another region safe thanks to you.",
    "Hopefully your new skill proved valuable.",
    "On to the next one!"
};
    [Header("Level 5 Dialogue")]
    [SerializeField]
    private string[] level5IntroDialogue =
    {
    "We've made it to the epicenter of the infection Guardian.",
    "This is the largest area yet, and the paths twist and turn.",
    "We must be vigilant and work fast to clear through this area.",
    "Onwards!!!!"
};

    [SerializeField]
    private string[] level5EndDialogue =
    {
    "Amazing work Guardian!",
    "That was the last of the infection!",
    "Let's celebrate!!!"
};
    #endregion

    private int currentCombo = 0;
    private float lastKillTime;
    private float comboTimer;
    private string comboType = "Bacteria";

    private PlayerMove playerMove;
    [HideInInspector] public float timeDelta;

    [HideInInspector] public bool displayUp;
    #endregion
    #region Unity Methods
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

        displayUp = GetComponent<InfoManager>().IsDisplayUp();

        timeDelta = Time.fixedDeltaTime;
        timeDelta = (float)Math.Round(timeDelta, 2);

        Debug.Log("TIM DELT: " + timeDelta);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sname = scene.name;

        if (sname == mainMenuScene)
        {
            ResetGameState();
            Button endGame = GameObject.Find("Confirm")?.GetComponent<Button>();
            if (endGame != null)
            {
                endGame.onClick.RemoveAllListeners();
                endGame.onClick.AddListener(CloseGame);
            }
            return;
        }

        bool isGameplayLevel = false;

        if (ProgressionManager.Instance != null)
            isGameplayLevel = ProgressionManager.Instance.IsGameplayScene(sname);
        else
            isGameplayLevel = sname.StartsWith("Level");

        if (isGameplayLevel && !levelRunning)
            StartLevel();

        GetComponent<InfoManager>().enabled = true;
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

        displayUp = GetComponent<InfoManager>().IsDisplayUp();
    }
    #endregion
    #region Helper Methods
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

        BacteriaAI[] bacteria = GameObject.FindObjectsByType<BacteriaAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        bacteriaCount = bacteria.Length;

        inflammationManager = InflammationManager.Instance;
        levelRunning = true;
        gameWon = false;
        gameLost = false;
        isPaused = false;

        string currentSceneName = SceneManager.GetActiveScene().name;
        timeRemaining = GetTimeLimitForScene(currentSceneName);
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

            currentSceneName = SceneManager.GetActiveScene().name;
            string[] currentIntroDialogue = GetIntroDialogueForScene(currentSceneName);

            dialogueManager.SetDialogueLines(currentIntroDialogue);
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

        onGameplayBegin?.Invoke();

        Debug.Log("Gameplay started after dialogue.");
    }

    public void StartEndDialogue()
    {
        levelRunning = false;
        timerStarted = false;
        CancelInvoke(nameof(TickTimer));
        onGameplayEnd?.Invoke();

        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();

        if (dialogueManager != null)
        {
            waitingForEndDialogue = true;

            dialogueManager.onDialogueEnd.RemoveListener(BeginGameplay);
            dialogueManager.onDialogueEnd.RemoveListener(HandleEndDialogueFinished);
            dialogueManager.onDialogueEnd.AddListener(HandleEndDialogueFinished);

            string currentSceneName = SceneManager.GetActiveScene().name;
            string[] currentEndDialogue = GetEndDialogueForScene(currentSceneName);

            dialogueManager.SetDialogueLines(currentEndDialogue);
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

        Debug.Log("HandleEndDialogueFinished START");
        if (ProgressionManager.Instance == null)
        {
            Debug.LogError("Recreating ProgressionManager");
            GameObject progObj = new GameObject("ProgressionManager");
            ProgressionManager prog = progObj.AddComponent<ProgressionManager>();
            DontDestroyOnLoad(progObj);
        }

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

        GoNext();
    }

    private string[] GetIntroDialogueForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Level1":
                return level1IntroDialogue;

            case "Level2":
                return level2IntroDialogue;

            case "Level3":
                return level3IntroDialogue;

            case "Level4":
                return level4IntroDialogue;

            case "Level5":
                return level5IntroDialogue;

            default:
                return level1IntroDialogue;
        }
    }

    private string[] GetEndDialogueForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Level1":
                return level1EndDialogue;

            case "Level2":
                return level2EndDialogue;

            case "Level3":
                return level3EndDialogue;

            case "Level4":
                return level4EndDialogue;

            case "Level5":
                return level5EndDialogue;

            default:
                return level1EndDialogue;
        }
    }

    private float GetTimeLimitForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Level1":
                return 300f;

            case "Level2":
                return 210f;

            case "Level3":
                return 420f;

            case "Level4":
                return 300f;

            case "Level5":
                return 300f;

            default:
                return levelTimeLimit;
        }
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
        onGameplayEnd?.Invoke();
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
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Debug.Log("Game Unpaused");

        if (!displayUp)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }

    public void TogglePause()
    {
        if (SceneManager.GetActiveScene().name == "Tutorial") return;
        if (isPaused) UnpauseGame();
        else PauseGame();
    }

    public void GoHome()
    {
        ResetGameState();
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void GoNext()
    {
        if (ProgressionManager.Instance == null)
        {
            GoHome();
            return;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        int completedLevelIndex = ProgressionManager.Instance.GetLevelIndexFromScene(currentSceneName);

        if (completedLevelIndex <= 0)
        {
            GoHome();
            return;
        }

        bool isLastLevel = completedLevelIndex >= ProgressionManager.Instance.MaxLevelCount;

        ProgressionManager.Instance.MarkLevelCompleted(completedLevelIndex);

        ResetGameState();
        Time.timeScale = 1f;

        Debug.Log($"Scene: {currentSceneName}, Index: {completedLevelIndex}, Max: {ProgressionManager.Instance.MaxLevelCount}, isLast: {isLastLevel}");
        if (!isLastLevel) Debug.Log($"Loading next: {ProgressionManager.Instance.GetCurrentLevelScene()}");

        if (isLastLevel)
        {
            SceneManager.LoadScene("Ending");
            return;
        }

        SceneManager.LoadScene(ProgressionManager.Instance.GetCurrentLevelScene());
    }

    public void SwitchMode()
    {
        InfoDisplay[] infoArr = GameObject.FindObjectsByType<InfoDisplay>(FindObjectsSortMode.None);
        Debug.Log(infoArr.Length + " info displays were found");

        if(infoArr == null || infoArr.Length == 0)
        {
            Debug.LogWarning("No info objects found.");
            return;
        }

        foreach (InfoDisplay info in infoArr)
        {
            info.gameObject.SendMessage("Display");
        }
        infoMode = !infoMode;
        Debug.Log("Mode Switched.");
    }

    public bool IsDisplayOn() { return infoMode; }

    private IEnumerator fadeScene()
    {
        fadeController.FadeIn();
        yield return new WaitForSeconds(5f);
    }
    #endregion

    public void CloseGame()
    {
#if UNITY_EDITOR
UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}

