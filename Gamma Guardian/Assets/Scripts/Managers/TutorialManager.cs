using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;
using UnityEngine.IO;

public enum TutorialPhase { Movement = 1, Gammas = 2, Bacteria = 3, Mixed = 4 }

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    public DialogueManager dialogueManager;
    public GameObject medicineButton;
    public GameObject completionBar;
    public GameObject completionBarFill;
    public GameObject map;
    public TextMeshProUGUI killCounterUI;
    public ParticleSystem starEffect;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button playButton;
    [SerializeField] private PlayerInput playerInput;

    public GameObject cytokine;
    public FadeController fadeController;
    public GameObject immuneCellPrefab;
    public float immuneCellApproachDistance = 3f;

    [Header("Flash Effect")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.5f;
    public int flashLoops = 3;

    [Header("Tutorial Settings")]
    public Transform player;
    public float detectionDistance = 0.1f;

    [Header("Demo UI")]
    public TutorialMinimap minimap;
    public Vector2[] demoBacteriaPositions = { new Vector2(20, 10), new Vector2(-15, 25), new Vector2(10, -20) };

    [Header("Mini-Level Spawning")]
    public GameObject gammaPrefab;
    public GameObject bacteriaPrefab;
    public Transform[] spawnPoints;

    private Vector3 initialPlayerPos;
    private HashSet<Vector2> movedDirections = new HashSet<Vector2>();
    private bool hasCalmedGamma = false;
    private bool hasDefeatedBacteria = false;

    public TutorialPhase phase = TutorialPhase.Movement;
    private int phaseKills = 0;
    private int totalKills = 0;
    private int tutorialStep = 0;
    private bool isPaused = false;

    // --- Dialogue ---

    private string[] welcomeDialogue = {
        "Welcome to the body, Guardian Explorer! This is Central Body Command here. Let's do some training.",
        "Your goal is to help the immune system defeat the infection.",
        "This will allow us to defend the body against these invaders."
    };

    private string[] movementDialogueAndroid = {
        "First, learn to move: try forwards, backwards, up, and down.",
        "Use the joystick to move around!",
        "Try flying in a circle!"
    };

    private string[] movementDialoguePC = {
        "First, learn to move: try forwards, backwards, up, and down.",
        "Use W for up, A for left, D for right, and S for down.",
        "Try flying in a circle!"
    };

    private string[] immuneCellDialogue = {
        /*"Nice work!*/ "This is an immune cell. They fight invaders. They are <b>green</b> on the map.",
        "They call cytokines for backup, but too many cause chaos."
    };

    private string[] gammaPhaseDialogue = {
        "This is an interferon gamma - a cytokine that excites immune cells.",
        "Too many and the cells go haywire! Calm them with the medicine button.",
        "Watch the <b>inflammation bar</b> - it rises with inflammation!"
    };

    private string[] gammaCalmedDialogue = {
        "Nice! Find the next cytokine! They are <b>blue</b> on the map."
    };

    private string[] bacteriaPhaseDialogue = {
        "Bacteria incoming! Immune cells need your help.",
        "Check the <b>map</b>! <b>Red dots</b> show bacteria locations.",
        "Fly to each red dot and use the medicine button to defeat them!"
    };

    private string[] mixedPhaseDialogue = {
        "Final test: gammas AND bacteria at once!",
        "Balance calming cytokines and defeating bacteria.",
        "You've got this, Guardian Explorer!"
    };

    private string[] endingDialogue = {
        "<b>Hunt red dots, watch the bar, and don't miss any bacteria!</b>",
        "Good luck Guardian! Only you can save the body!"
    };

    private string[] calmGammaDialogueAndroid = {
        "To calm the gamma, fly up to it and press the medicine button.",
        "The medicine button is the square on the right."
    };

    private string[] calmGammaDialoguePC = {
        "To calm the gamma, fly up to it and press the medicine button or press space.",
        "The medicine button is the square on the right."
    };

    //Cam Movement stuff
    private Transform playerCam;
    private Vector3 targetDir; //direction to move camera
    private bool isCameraMoving = false;
    private float camMoveDist = 0;
    private float timeDelta;


    // --- Start ---

    void Start()
    {
        fadeController.FadeOut();
        immuneCellPrefab.SetActive(false);
        cytokine.SetActive(false);
        medicineButton.SetActive(false);
        completionBar.SetActive(false);
        map.SetActive(false);
        killCounterUI.gameObject.SetActive(false);

        initialPlayerPos = player.position;
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        dialogueManager.SetDialogueLines(welcomeDialogue);
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(OnWelcomeEnd);

        playerCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        timeDelta = Time.fixedDeltaTime;

        Debug.Log("camPos: " + playerCam.position);
    }

    // --- Update ---

    void Update()
    {
        if (tutorialStep == 1)
            CheckMovement();
        else if (tutorialStep == 6)
            CheckImmuneCellApproach();
        if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                     || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame))
        {
            TogglePause();
        }

        //Camera movement
        if (isCameraMoving)
        {
            if (camMoveDist >= targetDir.magnitude)
            {
                isCameraMoving = false;
                return;
            }

            if (camMoveDist < .6f * targetDir.magnitude)
            {
                playerCam.position += targetDir * 1.25f * timeDelta; //2.5
                camMoveDist += targetDir.magnitude * 1.25f * timeDelta;
            }
            else if (camMoveDist < .9f * targetDir.magnitude)
            {
                playerCam.position += targetDir * .75f * timeDelta; //1.75
                camMoveDist += targetDir.magnitude * .75f * timeDelta;
            }
            else
            {
                playerCam.position += targetDir * .25f * timeDelta; //.5
                camMoveDist += targetDir.magnitude * .25f * timeDelta;
            }
        }
    }

    // --- Phase 1: Movement ---

    void OnWelcomeEnd()
    {
        Debug.Log("OnWelcomeEnd test");
        tutorialStep = 1;
        dialogueManager.onDialogueEnd.RemoveListener(OnWelcomeEnd);
#if UNITY_ANDROID || UNITY_IOS
        dialogueManager.SetDialogueLines(movementDialogueAndroid);
#else
        dialogueManager.SetDialogueLines(movementDialoguePC);
#endif
        dialogueManager.StartDialogue();
    }

    void CheckMovement()
    {
        player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        //Debug.Log("CheckMovement test");
        Vector3 currentPos = player.position;
        if (Vector3.Distance(initialPlayerPos, currentPos) > detectionDistance)
        {
            Vector2 dir = new Vector2(
                Mathf.RoundToInt(Mathf.Sign(currentPos.x - initialPlayerPos.x)),
                Mathf.RoundToInt(Mathf.Sign(currentPos.y - initialPlayerPos.y))
            );
            if (dir != Vector2.zero) movedDirections.Add(dir);
            initialPlayerPos = currentPos;

            if (movedDirections.Count >= 4)
                SpawnImmuneCellApproach();
        }
    }

    void SpawnImmuneCellApproach()
    {
        StartCoroutine(MoveCamToObj(immuneCellPrefab));

        Debug.Log("SpawnImmuneCellApproach test");
        tutorialStep = 6;
        immuneCellPrefab.SetActive(true);
        //dialogueManager.SetDialogueLines(new string[] { "Follow the arrow back towards the center." });
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(OnImmuneApproachDialogueEnd);
    }

    IEnumerator MoveCamToObj(GameObject obj)
    {
        Vector3 immunePos = obj.transform.position;
        targetDir = new Vector3(immunePos.x, immunePos.y, playerCam.position.z) - playerCam.position;
        SetMovement(false);
        isCameraMoving = true;
        camMoveDist = 0;

        //yield return new WaitForSeconds(1f);
        dialogueManager.SetDialogueLines(new string[] { "Okay, now please return to the center." }); //"Follow the arrow back towards the center."
        yield return new WaitForSeconds(2f);

        //immunePos = immuneCellPrefab.transform.position;
        targetDir = -targetDir; //playerCam.position - new Vector3(immunePos.x, immunePos.y, playerCam.position.z);
        isCameraMoving = true;
        camMoveDist = 0;
        yield return new WaitForSeconds(1f);

        SetMovement(true);
    }

    void SetMovement(bool moving)
    {
        RigidbodyConstraints2D rbCon = (moving) ? RigidbodyConstraints2D.FreezeRotation : 
            (RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation);

        Rigidbody2D playerRB = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        playerRB.constraints = rbCon;
        immuneCellPrefab.GetComponent<Rigidbody2D>().constraints = rbCon;
        cytokine.GetComponent<Rigidbody2D>().constraints = rbCon;
    }

    void OnImmuneApproachDialogueEnd() { Debug.Log("OnImmuneApproachDialogueEnd test"); }

    void CheckImmuneCellApproach()
    {
        //Debug.Log("CheckImmuneCellApproach test");
        float dist = Vector3.Distance(player.position, immuneCellPrefab.transform.position);
        if (dist <= immuneCellApproachDistance)
            OnImmuneCellReached();
    }

    void OnImmuneCellReached()
    {
        Debug.Log("OnImmuneCellReached test");
        tutorialStep = 0;
        dialogueManager.onDialogueEnd.RemoveAllListeners();
        map.SetActive(true);
        FlashUIColor(map);
        dialogueManager.SetDialogueLines(immuneCellDialogue);
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(StartGammaPhase);
    }

    // --- Phase 2: Gammas ---

    void StartGammaPhase()
    {
        Debug.Log("StartGammaPhase test");
        dialogueManager.onDialogueEnd.RemoveAllListeners();
        phase = TutorialPhase.Gammas;
        phaseKills = 0;
        cytokine.SetActive(true);

        StartCoroutine(MoveCamToObj(cytokine));

        medicineButton.SetActive(true);
        FlashUIColor(medicineButton);

        completionBar.SetActive(true);
        killCounterUI.gameObject.SetActive(true);
        killCounterUI.text = "Gammas Calmed: 0/2";

        dialogueManager.SetDialogueLines(gammaPhaseDialogue);
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(OnGammaDialogueEnd);
    }

    void OnGammaDialogueEnd()
    {
        Debug.Log("OnGammaDialogueEnd test");
        dialogueManager.onDialogueEnd.RemoveListener(OnGammaDialogueEnd);
        StartCoroutine(BarDemo(() =>
        {
#if UNITY_ANDROID || UNITY_IOS
            dialogueManager.SetDialogueLines(calmGammaDialogueAndroid);
#else
            dialogueManager.SetDialogueLines(calmGammaDialoguePC);
#endif
            dialogueManager.StartDialogue();
        }));
    }

    IEnumerator BarDemo(System.Action onComplete)
    {
        Debug.Log("BarDemo test");

        InflammationBar barScript = completionBarFill.GetComponent<InflammationBar>();
        barScript.SetInflammation(0.1f);
        yield return new WaitForSeconds(0.5f);
        DOTween.To(() => 0.1f, barScript.SetInflammation, 0.7f, 1.2f).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(1.5f);
        DOTween.To(() => 0.7f, barScript.SetInflammation, 0.1f, 1.2f).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(1.5f);
        onComplete?.Invoke();
    }

    public void OnGammaCalmed()
    {
        Debug.Log("OnGammaCalmed test");

        if (phase != TutorialPhase.Gammas && phase != TutorialPhase.Mixed) return;
        OnEnemyKilled();

        if (phase == TutorialPhase.Gammas)
        {
            dialogueManager.SetDialogueLines(gammaCalmedDialogue);
            dialogueManager.StartDialogue();

            if (phaseKills < 2)
                SpawnEnemies("gamma", 1);
        }
    }

    // --- Phase 3: Bacteria ---

    void StartBacteriaPhase()
    {
        Debug.Log("StartBacteriaPhase test");

        phase = TutorialPhase.Bacteria;
        phaseKills = 0;
        killCounterUI.text = "Bacteria Defeated: 0/3";

        dialogueManager.onDialogueEnd.RemoveAllListeners();
        dialogueManager.SetDialogueLines(bacteriaPhaseDialogue);
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(OnBacteriaDialogueEnd);
    }

    void OnBacteriaDialogueEnd()
    {
        Debug.Log("OnBacteriaDialogueEnd test");

        dialogueManager.onDialogueEnd.RemoveListener(OnBacteriaDialogueEnd);
        SpawnEnemies("bacteria", 1);
    }

    public void OnBacteriaDefeated()
    {
        Debug.Log("OnBacteriaDefeated test");

        if (phase != TutorialPhase.Bacteria && phase != TutorialPhase.Mixed) return;
        OnEnemyKilled();

        if (phase == TutorialPhase.Bacteria && phaseKills < 3)
            SpawnEnemies("bacteria", 1);
    }

    // --- Phase 4: Mixed ---

    void StartMixedPhase()
    {
        Debug.Log("StartMixedPhase test");

        phase = TutorialPhase.Mixed;
        phaseKills = 0;
        killCounterUI.text = $"Total Kills: {totalKills}/9";

        dialogueManager.onDialogueEnd.RemoveAllListeners();
        dialogueManager.SetDialogueLines(mixedPhaseDialogue);
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(OnMixedDialogueEnd);
    }

    void OnMixedDialogueEnd()
    {
        Debug.Log("StartMixedPhase test");

        dialogueManager.onDialogueEnd.RemoveListener(OnMixedDialogueEnd);
        SpawnEnemies("gamma", 2);
        SpawnEnemies("bacteria", 2);
    }

    // --- Kill Tracking ---

    void OnEnemyKilled()
    {
        Debug.Log("OnEnemyKilled test");

        phaseKills++;
        totalKills++;
        if (starEffect != null) starEffect.Play();
        UpdateKillCounter();
        CheckPhaseComplete();
    }

    void UpdateKillCounter()
    {
        Debug.Log("UpdateKillCounter test");

        if (phase == TutorialPhase.Gammas)
            killCounterUI.text = $"Gammas Calmed: {phaseKills}/2";
        else if (phase == TutorialPhase.Bacteria)
            killCounterUI.text = $"Bacteria Defeated: {phaseKills}/3";
        else if (phase == TutorialPhase.Mixed)
            killCounterUI.text = $"Total Kills: {totalKills}/9";
    }

    void CheckPhaseComplete()
    {
        Debug.Log("CheckPhaseComplete test");

        if (phase == TutorialPhase.Gammas && phaseKills >= 2)
            StartBacteriaPhase();
        else if (phase == TutorialPhase.Bacteria && phaseKills >= 3)
            StartMixedPhase();
        else if (phase == TutorialPhase.Mixed && totalKills >= 9)
            Victory();
    }

    // --- Spawning ---

    void SpawnEnemies(string type, int count)
    {
        Debug.Log("SpawnEnemies test");

        GameObject prefab = type == "gamma" ? gammaPrefab : bacteriaPrefab;
        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        }
    }

    // --- Helpers ---

    void FlashUIColor(GameObject uiElement)
    {
        Debug.Log("FlashUIColor test");

        Graphic[] graphics = uiElement.GetComponentsInChildren<Graphic>();
        Sequence flashSeq = DOTween.Sequence();
        foreach (Graphic g in graphics)
        {
            flashSeq.Join(DOTween.To(() => g.color, x => g.color = x, flashColor, flashDuration / 2)
                .SetLoops(flashLoops * 2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine));
        }
        flashSeq.Play();
    }

    // --- Victory & Load ---

    void Victory()
    {
        Debug.Log("Victory test");

        dialogueManager.onDialogueEnd.RemoveAllListeners();
        dialogueManager.SetDialogueLines(endingDialogue);
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(LoadLevel);
    }

    void LoadLevel()
    {
        dialogueManager.SetDialogueLines(new string[] { "" });
        dialogueManager.StartDialogue();
        StartCoroutine(LoadLevelCoroutine());
    }

    private IEnumerator LoadLevelCoroutine()
    {
        fadeController.FadeIn();
        yield return new WaitForSeconds(2f);

        bool completedBefore = ProgressionManager.Instance != null &&
                               ProgressionManager.Instance.HasCompletedTutorialBefore;

        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.MarkTutorialCompleted();

        SceneManager.LoadScene("Level1");
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        // Only react on performed, not started/canceled
        if (!context.performed) return;

        TogglePause();
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenu != null)
            pauseMenu.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
        AudioListener.pause = isPaused;
    }

    // Hook these to your pause menu buttons
    public void OnResumeButton()
    {
        if (!isPaused) return;
        TogglePause();
    }

    public void OnQuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
