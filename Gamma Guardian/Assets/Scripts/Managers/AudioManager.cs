using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioSource sfxSource;

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string tutorialScene = "Tutorial";
    [SerializeField] private string glossaryScene = "Glossary";
    [SerializeField] private string introCutsceneScene = "Intro";
    [SerializeField] private string outroCutsceneScene = "Ending";

    [Header("Level Scenes")]
    [SerializeField] private string[] levelScenes;

    [Header("Playlists")]
    [SerializeField] private AudioClip[] mainMenuPlaylist;
    [SerializeField] private AudioClip[] levelPlaylist;

    [Header("Cutscene Tracks")]
    [SerializeField] private AudioClip introCutsceneTrack;
    [SerializeField] private AudioClip outroCutsceneTrack;
    [SerializeField] private AudioClip tutorialTrack;
    [SerializeField] private AudioClip glossaryTrack;

    [Header("Music Settings")]
    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private float crossfadeDuration = 1.25f;
    [SerializeField] private float nextTrackLeadTime = 0.15f;
    [SerializeField] private bool shuffleMainMenuPlaylist = true;
    [SerializeField] private bool shuffleLevelPlaylist = true;

    private enum MusicMode
    {
        None,
        MainMenu,
        Level,
        IntroCutscene,
        OutroCutscene,
        Tutorial,
        Glossary
    }

    private MusicMode currentMode = MusicMode.None;
    private AudioSource activeMusicSource;
    private AudioSource inactiveMusicSource;
    private Coroutine playlistCoroutine;
    private Coroutine fadeCoroutine;

    private List<AudioClip> mainMenuQueue = new List<AudioClip>();
    private List<AudioClip> levelQueue = new List<AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSourceA != null) musicSourceA.outputAudioMixerGroup = musicGroup;
        if (musicSourceB != null) musicSourceB.outputAudioMixerGroup = musicGroup;
        if (sfxSource != null) sfxSource.outputAudioMixerGroup = sfxGroup;

        musicSourceA.playOnAwake = false;
        musicSourceB.playOnAwake = false;
        sfxSource.playOnAwake = false;

        musicSourceA.loop = false;
        musicSourceB.loop = false;

        musicSourceA.volume = 0f;
        musicSourceB.volume = 0f;

        activeMusicSource = musicSourceA;
        inactiveMusicSource = musicSourceB;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        HandleScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleScene(scene.name);
    }

    private void HandleScene(string sceneName)
    {
        if (sceneName == mainMenuScene)
        {
            if (currentMode != MusicMode.MainMenu)
            {
                currentMode = MusicMode.MainMenu;
                StartPlaylist(mainMenuPlaylist, mainMenuQueue, shuffleMainMenuPlaylist);
            }
            return;
        }

        if (sceneName == glossaryScene)
        {
            if (currentMode != MusicMode.Glossary)
            {
                currentMode = MusicMode.Glossary;
                PlaySingleTrack(glossaryTrack);
            }
            return;
        }

        if (sceneName == tutorialScene)
        {
            if (currentMode != MusicMode.Tutorial)
            {
                currentMode = MusicMode.Tutorial;
                PlaySingleTrack(tutorialTrack);
            }
            return;
        }

        if (sceneName == introCutsceneScene)
        {
            if (currentMode != MusicMode.IntroCutscene)
            {
                currentMode = MusicMode.IntroCutscene;
                PlaySingleTrack(introCutsceneTrack);
            }
            return;
        }

        if (sceneName == outroCutsceneScene)
        {
            if (currentMode != MusicMode.OutroCutscene)
            {
                currentMode = MusicMode.OutroCutscene;
                PlaySingleTrack(outroCutsceneTrack);
            }
            return;
        }

        if (IsLevelScene(sceneName))
        {
            if (currentMode != MusicMode.Level)
            {
                currentMode = MusicMode.Level;
                StartPlaylist(levelPlaylist, levelQueue, shuffleLevelPlaylist);
            }
        }
        else
        {
            StartPlaylist(mainMenuPlaylist, mainMenuQueue, shuffleMainMenuPlaylist);
        }
    }

    private bool IsLevelScene(string sceneName)
    {
        foreach (string s in levelScenes)
        {
            if (s == sceneName)
                return true;
        }
        return false;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlaySingleTrack(AudioClip clip)
    {
        if (clip == null) return;

        StopPlaylistRoutine();

        if (activeMusicSource.clip == clip && activeMusicSource.isPlaying)
            return;

        CrossfadeToClip(clip);
    }

    public void StartPlaylist(AudioClip[] playlist, List<AudioClip> queue, bool shuffle)
    {
        if (playlist == null || playlist.Length == 0) return;

        StopPlaylistRoutine();
        playlistCoroutine = StartCoroutine(PlaylistLoop(playlist, queue, shuffle));
    }

    private IEnumerator PlaylistLoop(AudioClip[] playlist, List<AudioClip> queue, bool shuffle)
    {
        while (true)
        {
            AudioClip nextClip = GetNextClip(playlist, queue, shuffle);
            if (nextClip == null)
                yield break;

            bool alreadyPlayingThis =
                activeMusicSource.clip == nextClip &&
                activeMusicSource.isPlaying;

            if (!alreadyPlayingThis)
            {
                CrossfadeToClip(nextClip);
            }

            float waitTime = Mathf.Max(0.1f, nextClip.length - nextTrackLeadTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private AudioClip GetNextClip(AudioClip[] playlist, List<AudioClip> queue, bool shuffle)
    {
        if (playlist == null || playlist.Length == 0)
            return null;

        if (!shuffle)
        {
            if (queue.Count == 0)
            {
                queue.AddRange(playlist);
            }

            AudioClip clip = queue[0];
            queue.RemoveAt(0);
            return clip;
        }

        if (queue.Count == 0)
        {
            queue.AddRange(playlist);
            Shuffle(queue);

            if (playlist.Length > 1 && activeMusicSource != null && activeMusicSource.clip != null && queue[0] == activeMusicSource.clip)
            {
                AudioClip first = queue[0];
                queue.RemoveAt(0);
                queue.Add(first);
            }
        }

        AudioClip next = queue[0];
        queue.RemoveAt(0);
        return next;
    }

    private void Shuffle(List<AudioClip> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            AudioClip temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void CrossfadeToClip(AudioClip newClip)
    {
        if (newClip == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(CrossfadeCoroutine(newClip));
    }

    private IEnumerator CrossfadeCoroutine(AudioClip newClip)
    {
        inactiveMusicSource.clip = newClip;
        inactiveMusicSource.volume = 0f;
        inactiveMusicSource.loop = false;
        inactiveMusicSource.Play();

        float startActiveVolume = activeMusicSource.isPlaying ? activeMusicSource.volume : 0f;
        float timer = 0f;

        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / crossfadeDuration);

            inactiveMusicSource.volume = Mathf.Lerp(0f, musicVolume, t);
            activeMusicSource.volume = Mathf.Lerp(startActiveVolume, 0f, t);

            yield return null;
        }

        inactiveMusicSource.volume = musicVolume;
        activeMusicSource.volume = 0f;
        activeMusicSource.Stop();
        activeMusicSource.clip = null;

        SwapMusicSources();
        fadeCoroutine = null;
    }

    private void SwapMusicSources()
    {
        AudioSource temp = activeMusicSource;
        activeMusicSource = inactiveMusicSource;
        inactiveMusicSource = temp;
    }

    private void StopPlaylistRoutine()
    {
        if (playlistCoroutine != null)
        {
            StopCoroutine(playlistCoroutine);
            playlistCoroutine = null;
        }
    }

    public void StopMusic(float fadeOutTime = 0.5f)
    {
        StopPlaylistRoutine();

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutBothCoroutine(fadeOutTime));
    }

    private IEnumerator FadeOutBothCoroutine(float duration)
    {
        float startA = musicSourceA.volume;
        float startB = musicSourceB.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            musicSourceA.volume = Mathf.Lerp(startA, 0f, t);
            musicSourceB.volume = Mathf.Lerp(startB, 0f, t);

            yield return null;
        }

        musicSourceA.Stop();
        musicSourceB.Stop();
        musicSourceA.clip = null;
        musicSourceB.clip = null;
        musicSourceA.volume = 0f;
        musicSourceB.volume = 0f;

        fadeCoroutine = null;
    }
}