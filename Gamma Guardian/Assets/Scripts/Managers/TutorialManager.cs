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

    [Header("Flash Effect")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.5f;
    public int flashLoops = 3;

    [Header("Tutorial Settings")]
    public Transform player;
    public float detectionDistance = 0.1f; // Small distance to detect movement

    private Vector3 initialPlayerPos;
    private HashSet<Vector2> movedDirections = new HashSet<Vector2>();
    private bool hasCalmedGamma = false;
    public int tutorialStep = 0;
    private bool hasDefeatedBacteria = false;

    private string[] welcomeDialogue = {
        "Welcome to the body, Guardian Explorer!",
        "Your goal is to calm the interferon gammas and destroy the infection.",
        "This will allow us to defend the body."
    };

    private string[] moveDialogue = {
        "First, learn to move: try forwards, backwards, up, and down.",
        "Use W for up, A for left, D for right, and S for down."
    };

    private string[] calmGammaDialogue = {
        "You see, the gammas are trying to help the body, but they get the immune cells too excited!",
        "When that happens, the cells get confused and start attacking the body instead!",
        "To calm the interferon gamma, press the medicine button.",
        "The medicine button is the small square on the left."
    };

    private string[] bacteriaDialogue = {
        "Excellent work! Now let's tackle defeating the bacteria.",
        "The immune cells in this patient do not respond correctly to infection.",
        "They try super hard! But they can't defeat the bacteria on their own.",
        "We have to help them. Use the medicine button to defeat the bacteria."
    };

    private string[] barDialogue1 =
    {
        "Perfect! Now that the bacteria is gone the body will be safe.",
        "If we don't defeat the bacteria, the patient won't be able to heal.",
        "Now, there's one last thing you need to know."
    };

    private string[] barDialogue2 =
    {
        "This flashing bar is the inflammation bar. It tells us how badly the body has inflammed.",
        "It will gradually increase and decrease as inflammation changes.",
        "Keep track of this bar, if it gets too full, it's game over.",
    };

    private string[] endingDialogue =
    {
        "Now Guardian Explorer, you have all you need to take on the infection and save the patient.",
        "Fly onwards and save them!"
    };

    void Start()
    {
        medicineButton.SetActive(false);
        completionBar.SetActive(false);
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
    }

    void OnWelcomeEnd()
    {
        tutorialStep = 1;
        dialogueManager.onDialogueEnd.RemoveListener(OnWelcomeEnd);
        dialogueManager.SetDialogueLines(moveDialogue);
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
                OnMovementComplete();
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

    void OnMovementComplete()
    {
        tutorialStep = 2;
        dialogueManager.SetDialogueLines(calmGammaDialogue);
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
        dialogueManager.onDialogueEnd.AddListener(OnTutorialComplete);
        completionBar.SetActive(true);
        FlashUIColor(completionBar);
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
        SceneManager.LoadScene("Level");
    }
}
