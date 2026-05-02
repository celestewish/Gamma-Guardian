using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    public DialogueManager dialogueManager;
    public GameObject medicineButton;
    public GameObject completionBar;
    public GameObject completionBarFill;
    public GameObject map;

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

    private Vector3 initialPlayerPos;
    private HashSet<Vector2> movedDirections = new HashSet<Vector2>();
    private bool hasCalmedGamma = false;
    public int tutorialStep = 0;
    private bool hasDefeatedBacteria = false;

    private string[] welcomeDialogue = {
        "Welcome to the body, Guardian Explorer! This is Central Body Command here and I have a new mission.",
        "Your goal is to help the immune system defeat the infection.",
        "This will allow us to defend the body against these invaders."
    };

    private string[] immuneCell = {
    "Nice work! This is an immune cell. They fight invaders.",
    "They call cytokines for backup, but too many cause chaos."
};

    private string[] bacteriaDialogue = {
        "Excellent work! Now let's tackle defeating the bacteria.",
        "The immune cells in this patient can't defeat the bacteria on their own.",
        "We have to help them. Fly up to the bacteria and use the medicine button."
    };

    private string[] barDialogue1 = {
    "Perfect! Clear all bacteria to heal the patient."
};

    private string[] barDialogue2 = {
    "Watch the inflammation bar. It rises quickly when inflammation does.",
    "Too full? Game over."
};

    private string[] mapDialogue = {
    "Red dots on map = bacteria. Clear them all to win!"
};

    private string[] endingDialogue = {
    "<b>Don't miss any bacteria in an area before flying on<b>",
    "Good luck Guardian! Only you can save the body!"
};

    void Start()
    {
        fadeController.FadeOut();
        immuneCellPrefab.SetActive(false);
        cytokine.SetActive(false);
        medicineButton.SetActive(false);
        completionBar.SetActive(false);
        map.SetActive(false);
        initialPlayerPos = player.position;
        dialogueManager.SetDialogueLines(welcomeDialogue);
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(OnWelcomeEnd);
        player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
    }

    void Update()
    {
        if (tutorialStep == 1) // Movement tutorial
        {
            CheckMovement();
        }
        else if (tutorialStep == 2) // Calm gamma
        {
            CheckCalmedGamma();
        }
        else if (tutorialStep == 3) // Bacteria tutorial
        {
            CheckBacteriaDefeated();
        }
        else if (tutorialStep == 6) // Immune cell approach
        {
            CheckImmuneCellApproach();
        }
    }

    private string[] GetMovementDialogue()
    {
#if UNITY_ANDROID || UNITY_IOS
        return new string[]
        {
        "First, learn to move: try forwards, backwards, up, and down.",
        "Use the joystick to move around!",
        "Try flying in a circle!" };
#else
        return new string[]
        {
        "First, learn to move: try forwards, backwards, up, and down.",
        "Use W for up, A for left, D for right, and S for down.",
        "Try flying in a circle!" };
#endif
    }

    private string[] GetCalmGammaDialogue()
    {
#if UNITY_ANDROID || UNITY_IOS
        return new string[] {
            "This is a interferon gamma. A special type of cytokine.",
        "To calm the gamma, fly up to the gamma and press the medicine button.",
        "The medicine button is the square on the right."
    };
#else
return new string[] {
            "This is a interferon gamma. A special type of cytokine.",
        "To calm the gamma, fly up to the gamma and press the medicine button or press space.",
        "The medicine button is the square on the right."
    };
#endif
    }
    void OnWelcomeEnd()
    {
        tutorialStep = 1;
        dialogueManager.onDialogueEnd.RemoveListener(OnWelcomeEnd);
        dialogueManager.SetDialogueLines(GetMovementDialogue());
        dialogueManager.StartDialogue();
    }

    void CheckMovement()
    {
        player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        Vector3 currentPos = player.position;
        if (Vector3.Distance(initialPlayerPos, currentPos) > detectionDistance)
        {
            Vector2 dir = new Vector2(
                Mathf.RoundToInt(Mathf.Sign(currentPos.x - initialPlayerPos.x)),
                Mathf.RoundToInt(Mathf.Sign(currentPos.y - initialPlayerPos.y))
            );
            if (dir != Vector2.zero)
            {
                movedDirections.Add(dir);
            }
            initialPlayerPos = currentPos; // Update to detect new movements

            if (movedDirections.Count >= 4) // Assume 4 directions detected
            {
                ImmuneCell();
            }
        }
    }

    void FlashUIColor(GameObject uiElement)
    {
        Graphic[] graphics = uiElement.GetComponentsInChildren<Graphic>();
        Sequence flashSeq = DOTween.Sequence();

        foreach (Graphic g in graphics)
        {
            Color originalColor = g.color;
            flashSeq.Join(DOTween.To(() => g.color, x => g.color = x, flashColor, flashDuration / 2)
                .SetLoops(flashLoops * 2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine));
        }
        flashSeq.Play();
    }

    void ImmuneCell()
    {
        tutorialStep = 6;
        immuneCellPrefab.SetActive(true);
        dialogueManager.SetDialogueLines(new string[] { "Follow the arrow back towards the center." });
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(OnImmuneApproachDialogueEnd);
    }
    void OnImmuneApproachDialogueEnd()
    {
        // Keep listening for approach completion
    }

    void CheckImmuneCellApproach()
    {
        float distanceToImmune = Vector3.Distance(player.position, immuneCellPrefab.transform.position);

        if (distanceToImmune <= immuneCellApproachDistance)
        {
            OnImmuneCellReached();
        }
    }
    void OnImmuneCellReached()
    {
        tutorialStep = 0; // Reset for next phase
        dialogueManager.SetDialogueLines(immuneCell); // Full immune cell dialogue
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(OnMovementComplete);
    }

    void OnMovementComplete()
    {
        dialogueManager.onDialogueEnd.RemoveAllListeners();
        tutorialStep = 2;
        cytokine.SetActive(true);
        dialogueManager.SetDialogueLines(GetCalmGammaDialogue());
        dialogueManager.StartDialogue();
        medicineButton.SetActive(true);
        FlashUIColor(medicineButton);
    }

    public void OnGammaCalmed() // Call this from CytokinesScript.Deactivate or wherever calming happens
    {
        if (tutorialStep == 2)
        {
            hasCalmedGamma = true;
        }
    }

    void CheckCalmedGamma()
    {
        if (hasCalmedGamma)
        {
            OnGammaComplete();
        }
    }

    void OnGammaComplete()
    {
        tutorialStep = 3;
        dialogueManager.SetDialogueLines(bacteriaDialogue);
        dialogueManager.StartDialogue();
    }
    public void OnBacteriaDefeated()
    {
        if (tutorialStep == 3)
        {
            hasDefeatedBacteria = true;
        }
    }

    void CheckBacteriaDefeated()
    {
        if (hasDefeatedBacteria)
        {
            StartBarTutorial();
        }
    }

    void StartBarTutorial()
    {
        tutorialStep = 4;
        dialogueManager.onDialogueEnd.RemoveAllListeners();
        dialogueManager.SetDialogueLines(barDialogue1);
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(EndBarTutorial);
    }

    void EndBarTutorial()
    {
        dialogueManager.onDialogueEnd.RemoveListener(EndBarTutorial);
        dialogueManager.SetDialogueLines(barDialogue2);
        dialogueManager.StartDialogue();
        completionBar.SetActive(true);
        FlashUIColor(completionBar);
        dialogueManager.onDialogueEnd.AddListener(() => StartCoroutine(BarThenMap()));
    }

    IEnumerator BarThenMap()
    {

        InflammationBar barScript = completionBarFill.GetComponent<InflammationBar>();

        // Smooth rise (0.2 ? 0.7, cytokines bad)
        barScript.SetInflammation(0.2f);
        DOTween.To(() => 0.2f, barScript.SetInflammation, 0.7f, 1.2f)
            .SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(1.5f);

        // Smooth fall (0.7 ? 0.1, safe)
        DOTween.To(() => 0.7f, barScript.SetInflammation, 0.1f, 1.2f)
            .SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(1.5f);

        MapTutorial();
    }

    void MapTutorial()
    {
        dialogueManager.onDialogueEnd.RemoveAllListeners();
        dialogueManager.SetDialogueLines(mapDialogue);
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(OnTutorialComplete);
        map.SetActive(true);
        FlashUIColor(map);
        minimap.SpawnDemoDots(demoBacteriaPositions);
    }

    void OnTutorialComplete()
    {
        tutorialStep = 5;
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

        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.MarkTutorialCompleted();

        SceneManager.LoadScene("Level1");
    }
}
