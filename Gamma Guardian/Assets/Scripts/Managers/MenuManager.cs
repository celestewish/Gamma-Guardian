using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Floating Title & Splash")]
    public RectTransform titleTransform;
    public RectTransform splashTransform;
    public float floatAmplitude = 10f;
    public float floatSpeed = 1f;

    [Header("Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button glossaryButton;
    public Button settingsCloseButton;

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

    // Internal
    Vector3 titleStartPos;
    Vector3 splashStartPos;
    bool settingsOpen = false;

    void Awake()
    {
        if (titleTransform != null) titleStartPos = titleTransform.anchoredPosition;
        if (splashTransform != null) splashStartPos = splashTransform.anchoredPosition;

        if (playButton != null) playButton.onClick.AddListener(OnPlayPressed);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsPressed);
        if (glossaryButton != null) glossaryButton.onClick.AddListener(OnGlossaryPressed);
        if (settingsCloseButton != null) settingsCloseButton.onClick.AddListener(OnSettingsClosePressed);

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
    }

    void Update()
    {
        // Floating up and down for title and main character splash
        float offset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        if (titleTransform != null)
        {
            titleTransform.anchoredPosition = titleStartPos + new Vector3(0f, offset, 0f);
        }
        if (splashTransform != null)
        {
            splashTransform.anchoredPosition = splashStartPos + new Vector3(0f, offset, 0f);
        }
    }

    void PlayClickFeedback(Transform buttonTransform, bool open)
    {
        if (buttonTransform != null)
        {
            StopAllCoroutines();
            StartCoroutine(SquashAndStretch(buttonTransform));
        }


        AudioClip clickSfx = (open) ? openSfx : closeSfx;

        if (uiAudioSource != null && clickSfx != null)
        {
            uiAudioSource.PlayOneShot(clickSfx);
        }
    }

    System.Collections.IEnumerator SquashAndStretch(Transform t)
    {
        Vector3 originalScale = t.localScale;
        Vector3 squashed = new Vector3(originalScale.x * 1.1f, originalScale.y * 0.9f, originalScale.z);
        Vector3 stretched = new Vector3(originalScale.x * 0.95f, originalScale.y * 1.05f, originalScale.z);

        float duration = 0.08f;
        float timer = 0f;

        // Squash
        while (timer < duration)
        {
            float lerp = timer / duration;
            t.localScale = Vector3.Lerp(originalScale, squashed, lerp);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // Stretch
        timer = 0f;
        while (timer < duration)
        {
            float lerp = timer / duration;
            t.localScale = Vector3.Lerp(squashed, stretched, lerp);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // Return
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

    // Button callbacks

    void OnPlayPressed()
    {
        PlayClickFeedback(playButton != null ? playButton.transform : null, true);
        SceneManager.LoadScene("Cutscene");
    }

    void OnSettingsPressed()
    {
        PlayClickFeedback(settingsButton != null ? settingsButton.transform : null, true);
        settingsOpen = true;
        if (settingsCanvas != null) settingsCanvas.SetActive(true);
    }

    void OnGlossaryPressed()
    {
        PlayClickFeedback(glossaryButton != null ? glossaryButton.transform : null, true);
        SceneManager.LoadScene("Glossary");
    }

    void OnSettingsClosePressed()
    {
        PlayClickFeedback(settingsCloseButton != null ? settingsCloseButton.transform : null, false);
        settingsOpen = false;
        if (settingsCanvas != null) settingsCanvas.SetActive(false);
    }
}
