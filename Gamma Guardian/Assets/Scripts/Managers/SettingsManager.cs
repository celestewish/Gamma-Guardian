using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Brightness Overlay (for 2D/UI)")]
    public CanvasGroup brightnessOverlay; // Full-screen black Image + CanvasGroup

    [Header("2D Scene Sprites (optional)")]
    public SpriteRenderer[] dimmableSprites; // Drag key sprites here; or find at runtime

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioSource musicAudioSource;
    public AudioMixer audioMixer;

    // Keys
    private const string BRIGHTNESS_KEY = "Brightness";
    private const string QUALITY_KEY = "Quality";
    private const string SFX_VOL_KEY = "SfxVolume";
    private const string MUSIC_VOL_KEY = "MusicVolume";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyBrightness();
        // Re-find dimmableSprites if needed: dimmableSprites = FindObjectsOfType<SpriteRenderer>();
    }

    public void SetBrightness(float value)
    {
        PlayerPrefs.SetFloat(BRIGHTNESS_KEY, value);
        PlayerPrefs.Save();
        ApplyBrightness();
    }

    public void SetSfxVolume(float sliderValue)  // 0-1 from slider
    {
        float dB = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;  // -80dB to 0dB
        audioMixer.SetFloat("SFX", dB);

        PlayerPrefs.SetFloat(SFX_VOL_KEY, sliderValue);
        PlayerPrefs.Save();

        // Keep source fallback
        if (uiAudioSource != null) uiAudioSource.volume = sliderValue;
    }

    public void SetMusicVolume(float sliderValue)
    {
        float dB = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        audioMixer.SetFloat("Music", dB);

        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, sliderValue);
        PlayerPrefs.Save();

        if (musicAudioSource != null) musicAudioSource.volume = sliderValue;
    }

    public void SetQuality(int index)
    {
        PlayerPrefs.SetInt(QUALITY_KEY, index);
        PlayerPrefs.Save();
        QualitySettings.SetQualityLevel(index);
    }

    void LoadSettings()
    {
        float brightness = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, 1f);
        SetBrightness(brightness);

        float sfxVol = PlayerPrefs.GetFloat(SFX_VOL_KEY, 0.75f);
        SetSfxVolume(sfxVol);

        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0.75f);
        SetMusicVolume(musicVol);

        int quality = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.names.Length - 1);
        SetQuality(quality);
    }

    void ApplyBrightness()
    {
        float brightness = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, 1f);

        CanvasGroup overlay = brightnessOverlay ??
            (brightnessOverlay = GameObject.Find("BrightnessOverlay")?.GetComponent<CanvasGroup>());

        if (overlay != null)
        {
            overlay.alpha = 1f - brightness;
            overlay.blocksRaycasts = false;
        }

        if (dimmableSprites == null || dimmableSprites.Length == 0)
            dimmableSprites = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        Color tint = Color.white * brightness;
        foreach (var sprite in dimmableSprites)
        {
            if (sprite != null) sprite.color = tint;
        }
    }

}
