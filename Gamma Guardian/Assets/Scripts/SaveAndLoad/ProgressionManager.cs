using UnityEngine;
using UnityEngine.SceneManagement;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance;

    private const string IntroSeenKey = "IntroSeen";
    private const string TutorialCompletedKey = "TutorialCompleted";
    private const string CurrentLevelKey = "CurrentLevel";
    private const string HighestUnlockedLevelKey = "HighestUnlockedLevel";
    private const string OutroSeenKey = "OutroSeen";

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string tutorialSceneName = "Tutorial";
    public string introCutscene = "Intro";

    [Header("Level Order")]
    public string[] levelSceneNames = { "Level1", "Level2", "Level3", "Level4", "Level5" };
    public bool IntroSeen => PlayerPrefs.GetInt(IntroSeenKey, 0) == 1;
    public bool TutorialCompleted => PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1;
    public bool HasCompletedTutorialBefore { get; private set; }
    public int CurrentLevel => PlayerPrefs.GetInt(CurrentLevelKey, 1);
    public int HighestUnlockedLevel => PlayerPrefs.GetInt(HighestUnlockedLevelKey, 1);
    public int MaxLevelCount => levelSceneNames.Length;
    public bool OutroSeen => PlayerPrefs.GetInt(OutroSeenKey, 0) == 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        if (!PlayerPrefs.HasKey(IntroSeenKey))
            PlayerPrefs.SetInt(IntroSeenKey, 0);

        if (!PlayerPrefs.HasKey(TutorialCompletedKey))
            PlayerPrefs.SetInt(TutorialCompletedKey, 0);

        if (!PlayerPrefs.HasKey(CurrentLevelKey))
            PlayerPrefs.SetInt(CurrentLevelKey, 1);

        if (!PlayerPrefs.HasKey(HighestUnlockedLevelKey))
            PlayerPrefs.SetInt(HighestUnlockedLevelKey, 1);

        if (!PlayerPrefs.HasKey(OutroSeenKey))
            PlayerPrefs.SetInt(OutroSeenKey, 0);

        PlayerPrefs.Save();
    }

    public void MarkIntroSeen()
    {
        PlayerPrefs.SetInt(IntroSeenKey, 1);
        PlayerPrefs.Save();
    }

    public void MarkTutorialCompleted()
    {
        if (!HasCompletedTutorialBefore)
        {
            HasCompletedTutorialBefore = true;
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
        }
    }

    public void MarkLevelCompleted(int completedLevel)
    {
        int clampedCompleted = Mathf.Clamp(completedLevel, 1, MaxLevelCount);
        int nextLevel = Mathf.Clamp(clampedCompleted + 1, 1, MaxLevelCount);
        int newHighestUnlocked = Mathf.Max(HighestUnlockedLevel, nextLevel);

        PlayerPrefs.SetInt(CurrentLevelKey, nextLevel);
        PlayerPrefs.SetInt(HighestUnlockedLevelKey, newHighestUnlocked);
        PlayerPrefs.Save();
    }

    public void MarkOutroSeen()
    {
        PlayerPrefs.SetInt(OutroSeenKey, 1);
        PlayerPrefs.Save();
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex >= 1 && levelIndex <= HighestUnlockedLevel;
    }

    public bool HasCompletedLevel(int levelIndex)
    {
        return levelIndex >= 1 && levelIndex < HighestUnlockedLevel;
    }

    public string GetCurrentLevelScene()
    {
        int clampedLevel = Mathf.Clamp(CurrentLevel, 1, MaxLevelCount);
        return levelSceneNames[clampedLevel - 1];
    }

    public string GetLevelScene(int levelIndex)
    {
        int clampedLevel = Mathf.Clamp(levelIndex, 1, MaxLevelCount);
        return levelSceneNames[clampedLevel - 1];
    }

    public string GetPlayScene()
    {
        if (!IntroSeen)
            return introCutscene;
        if (!TutorialCompleted)
            return tutorialSceneName;

        return GetCurrentLevelScene();
    }

    public void LoadPlayScene()
    {
        SceneManager.LoadScene(GetPlayScene());
    }

    public void LoadLevel(int levelIndex)
    {
        if (!IsLevelUnlocked(levelIndex))
            return;

        SceneManager.LoadScene(GetLevelScene(levelIndex));
    }
    public void LoadProgress()
    {
        HasCompletedTutorialBefore = PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt(IntroSeenKey, 0);
        PlayerPrefs.SetInt(TutorialCompletedKey, 0);
        PlayerPrefs.SetInt(CurrentLevelKey, 1);
        PlayerPrefs.SetInt(HighestUnlockedLevelKey, 1);
        PlayerPrefs.SetInt(OutroSeenKey, 0);
        PlayerPrefs.Save();
    }

    public void ResetProgressAndLoadStart()
    {
        ResetProgress();
        SceneManager.LoadScene(introCutscene);
    }

    public void ClearAllProgressAndSettings()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        InitializeDefaults();
    }
    public int GetLevelIndexFromScene(string sceneName)
    {
        for (int i = 0; i < levelSceneNames.Length; i++)
        {
            if (levelSceneNames[i] == sceneName)
                return i + 1;
        }

        return -1;
    }

    public bool IsGameplayScene(string sceneName)
    {
        return GetLevelIndexFromScene(sceneName) != -1;
    }
    public bool HasAnyProgress()
    {
        return TutorialCompleted || PlayerPrefs.GetInt(CurrentLevelKey, 1) > 1 || PlayerPrefs.GetInt(HighestUnlockedLevelKey, 1) > 1;
    }

    public int GetHighestUnlockedLevel()
    {
        return HighestUnlockedLevel;
    }

    public bool CanUseLevelSelect()
    {
        return TutorialCompleted;
    }
}