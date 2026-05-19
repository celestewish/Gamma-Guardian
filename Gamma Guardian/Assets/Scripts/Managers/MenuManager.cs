using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Floating Title & Splash")]
    public RectTransform titleTransform;
    public RectTransform splashTransform;
    public float floatAmplitude = 10f;
    public float floatSpeed = 1f;

    [Header("Main Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button glossaryButton;
    public Button settingsCloseButton;

    [Header("Play Menu")]
    public GameObject playMenuCanvas;
    public Button continueButton;
    public Button restartButton;
    public Button levelSelectButton;
    public Button playMenuCloseButton;

    [Header("Level Select Menu")]
    public GameObject levelSelectCanvas;
    public Button tutorialButton;
    public Button[] levelButtons = new Button[5];
    public Button levelSelectCloseButton;

    [Header("Settings Canvas")]
    public GameObject settingsCanvas;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip openSfx;
    public AudioClip closeSfx;

    [Header("Settings Controls")]
    public Slider brightnessSlider;
    public TMP_Dropdown qualityDropdown;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;
    public AudioSource musicAudioSource;

    [Header("Brightness Overlay")]
    public CanvasGroup brightnessOverlay;

    [Header("Fade")]
    public FadeController fadeController;

    private Vector3 titleStartPos;
    private Vector3 splashStartPos;
    private bool settingsOpen = false;
    private bool playMenuOpen = false;
    private bool levelSelectOpen = false;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReassignSfxSource();
    }

    private void ReassignSfxSource()
    {
        GameObject sfxObject = GameObject.Find("SFX");

        if (sfxObject != null)
        {
            uiAudioSource = sfxObject.GetComponent<AudioSource>();
        }
        else
        {
            Debug.LogWarning("MenuManager could not find a GameObject named SFX in this scene.");
            uiAudioSource = null;
        }
    }

    void Awake()
    {
        if (titleTransform != null) titleStartPos = titleTransform.anchoredPosition;
        if (splashTransform != null) splashStartPos = splashTransform.anchoredPosition;

        if (playButton != null) playButton.onClick.AddListener(OnPlayPressed);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsPressed);
        if (glossaryButton != null) glossaryButton.onClick.AddListener(OnGlossaryPressed);
        if (settingsCloseButton != null) settingsCloseButton.onClick.AddListener(OnSettingsClosePressed);

        if (continueButton != null) continueButton.onClick.AddListener(OnContinuePressed);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartPressed);
        if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelectPressed);
        if (tutorialButton != null) tutorialButton.onClick.AddListener(OnTutorialPressed);
        if (playMenuCloseButton != null) playMenuCloseButton.onClick.AddListener(ClosePlayMenu);

        if (levelSelectCloseButton != null) levelSelectCloseButton.onClick.AddListener(CloseLevelSelect);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int capturedIndex = i + 1;
            if (levelButtons[i] != null)
                levelButtons[i].onClick.AddListener(() => OnLevelButtonPressed(capturedIndex));
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 1f);
            brightnessSlider.onValueChanged.AddListener(value => SettingsManager.Instance.SetBrightness(value));
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SfxVolume", 1f);
            sfxVolumeSlider.onValueChanged.AddListener(value => SettingsManager.Instance.SetSfxVolume(value));
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.value = PlayerPrefs.GetInt("Quality", QualitySettings.names.Length - 1);
            qualityDropdown.onValueChanged.AddListener(index => SettingsManager.Instance.SetQuality(index));
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicVolumeSlider.onValueChanged.AddListener(value => SettingsManager.Instance.SetMusicVolume(value));
        }

        if (settingsCanvas != null) settingsCanvas.SetActive(false);
        if (playMenuCanvas != null) playMenuCanvas.SetActive(false);
        if (levelSelectCanvas != null) levelSelectCanvas.SetActive(false);
    }

    private void Start()
    {
        ReassignSfxSource();

        if (fadeController != null)
            fadeController.FadeOut();

        RefreshPlayMenuButtons();
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        if (titleTransform != null)
            titleTransform.anchoredPosition = titleStartPos + new Vector3(0f, offset, 0f);

        if (splashTransform != null)
            splashTransform.anchoredPosition = splashStartPos + new Vector3(0f, offset, 0f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (levelSelectOpen)
            {
                CloseLevelSelectToPlayMenu();
            }
            else if (playMenuOpen)
            {
                ClosePlayMenu();
            }
            else if (settingsOpen)
            {
                OnSettingsClosePressed();
            }
        }
    }

    void PlayClickFeedback(Transform buttonTransform, bool open)
    {
        if (buttonTransform != null)
            StartCoroutine(SquashAndStretch(buttonTransform));

        AudioClip clickSfx = open ? openSfx : closeSfx;

        if (uiAudioSource != null && clickSfx != null)
            uiAudioSource.PlayOneShot(clickSfx);
    }

    IEnumerator SquashAndStretch(Transform t)
    {
        Vector3 originalScale = t.localScale;
        Vector3 squashed = new Vector3(originalScale.x * 1.1f, originalScale.y * 0.9f, originalScale.z);
        Vector3 stretched = new Vector3(originalScale.x * 0.95f, originalScale.y * 1.05f, originalScale.z);

        float duration = 0.08f;
        float timer = 0f;

        while (timer < duration)
        {
            float lerp = timer / duration;
            t.localScale = Vector3.Lerp(originalScale, squashed, lerp);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        timer = 0f;
        while (timer < duration)
        {
            float lerp = timer / duration;
            t.localScale = Vector3.Lerp(squashed, stretched, lerp);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        timer = 0f;
        while (timer < duration)
        {
            float lerp = timer / duration;
            t.localScale = Vector3.Lerp(stretched, originalScale, lerp);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        t.localScale = originalScale;
    }

    void OnPlayPressed()
    {
        PlayClickFeedback(playButton != null ? playButton.transform : null, true);
        OpenPlayMenu();
    }

    void OpenPlayMenu()
    {
        playMenuOpen = true;
        levelSelectOpen = false;

        if (playMenuCanvas != null) playMenuCanvas.SetActive(true);
        if (levelSelectCanvas != null) levelSelectCanvas.SetActive(false);

        Canvas.ForceUpdateCanvases();
        RefreshPlayMenuButtons();
        Canvas.ForceUpdateCanvases();
    }

    void ClosePlayMenu()
    {
        PlayClickFeedback(playMenuCloseButton != null ? playMenuCloseButton.transform : null, false);
        playMenuOpen = false;

        if (playMenuCanvas != null) playMenuCanvas.SetActive(false);
    }

    void OnContinuePressed()
    {
        PlayClickFeedback(continueButton != null ? continueButton.transform : null, true);

        if (ProgressionManager.Instance == null || !ProgressionManager.Instance.HasAnyProgress())
            return;

        StartCoroutine(LoadLevelCoroutine(ProgressionManager.Instance.GetPlayScene()));
    }

    void OnRestartPressed()
    {
        PlayClickFeedback(restartButton != null ? restartButton.transform : null, true);

        if (ProgressionManager.Instance == null)
            return;
        ProgressionManager.Instance.ResetProgress();
        StartCoroutine(LoadLevelCoroutine(ProgressionManager.Instance.GetPlayScene()));
    }

    void OnLevelSelectPressed()
    {
        PlayClickFeedback(levelSelectButton != null ? levelSelectButton.transform : null, true);

        if (ProgressionManager.Instance == null || !ProgressionManager.Instance.CanUseLevelSelect())
            return;

        OpenLevelSelect();
    }

    void OpenLevelSelect()
    {
        levelSelectOpen = true;
        playMenuOpen = false;

        if (playMenuCanvas != null) playMenuCanvas.SetActive(false);
        if (levelSelectCanvas != null) levelSelectCanvas.SetActive(true);

        Canvas.ForceUpdateCanvases();
        RefreshLevelButtons();
        Canvas.ForceUpdateCanvases();
    }

    void CloseLevelSelect()
    {
        PlayClickFeedback(levelSelectCloseButton != null ? levelSelectCloseButton.transform : null, false);
        levelSelectOpen = false;

        if (levelSelectCanvas != null) levelSelectCanvas.SetActive(false);
    }

    void CloseLevelSelectToPlayMenu()
    {
        PlayClickFeedback(levelSelectCloseButton != null ? levelSelectCloseButton.transform : null, false);
        levelSelectOpen = false;

        if (levelSelectCanvas != null) levelSelectCanvas.SetActive(false);

        OpenPlayMenu();
    }

    void OnLevelButtonPressed(int levelIndex)
    {
        if (ProgressionManager.Instance == null)
            return;

        if (!ProgressionManager.Instance.IsLevelUnlocked(levelIndex))
            return;

        if (levelButtons[levelIndex - 1] != null)
            PlayClickFeedback(levelButtons[levelIndex - 1].transform, true);

        StartCoroutine(LoadLevelCoroutine(ProgressionManager.Instance.GetLevelScene(levelIndex)));
    }

    void RefreshPlayMenuButtons()
    {
        if (ProgressionManager.Instance == null)
        {
            if (continueButton != null) continueButton.interactable = false;
            if (levelSelectButton != null) levelSelectButton.interactable = false;
            return;
        }

        bool hasProgress = ProgressionManager.Instance.HasAnyProgress();
        bool canUseLevelSelect = ProgressionManager.Instance.CanUseLevelSelect();

        if (continueButton != null)
            continueButton.interactable = hasProgress;

        if (levelSelectButton != null)
            levelSelectButton.interactable = canUseLevelSelect;
    }

    void RefreshLevelButtons()
    {
        if (ProgressionManager.Instance == null)
        {
            for (int i = 0; i < levelButtons.Length; i++)
            {
                if (levelButtons[i] != null)
                    levelButtons[i].interactable = false;
            }
            return;
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] != null)
                levelButtons[i].interactable = ProgressionManager.Instance.IsLevelUnlocked(i + 1);
        }
    }

    void OnSettingsPressed()
    {
        PlayClickFeedback(settingsButton != null ? settingsButton.transform : null, true);
        settingsOpen = true;

        if (settingsCanvas != null)
            settingsCanvas.SetActive(true);
    }

    void OnGlossaryPressed()
    {
        PlayClickFeedback(glossaryButton != null ? glossaryButton.transform : null, true);
        StartCoroutine(LoadLevelCoroutine("Glossary"));
    }

    void OnSettingsClosePressed()
    {
        PlayClickFeedback(settingsCloseButton != null ? settingsCloseButton.transform : null, false);
        settingsOpen = false;

        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);
    }

    void OnTutorialPressed()
    {
        PlayClickFeedback(tutorialButton != null ? tutorialButton.transform : null, false);
        StartCoroutine(LoadLevelCoroutine("Tutorial"));
    }

    private IEnumerator LoadLevelCoroutine(string levelName)
    {
        if (fadeController != null)
            fadeController.FadeIn();

        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(levelName);
    }
}