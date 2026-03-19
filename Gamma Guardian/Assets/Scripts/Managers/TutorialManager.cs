using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    public DialogueManager dialogueManager;
    public GameObject inflammationMeter; // Set active when needed

    [Header("Tutorial Settings")]
    public Transform player;
    public float detectionDistance = 0.1f; // Small distance to detect movement

    private Vector3 initialPlayerPos;
    private HashSet<Vector2> movedDirections = new HashSet<Vector2>();
    private bool hasCalmedGamma = false;
    private int tutorialStep = 0;

    private string[] welcomeDialogue = {
        "Welcome to the body, Guardian Explorer!",
        "Your goal is to calm the interferon gammas and immune cells.",
        "This will let them destroy bacteria without too much inflammation."
    };

    private string[] moveDialogue = {
        "First, learn to move: try forwards, backwards, up, and down.",
        "Use W for up, A for left, D for right, and S for down."
    };

    private string[] calmGammaDialogue = {
        "Great! Now, use the medicine button to calm an interferon gamma.",
        "The medicine button is the small square on the left."
    };

    private string[] inflammationDialogue = {
        "Excellent work! Now let's tackle inflammation.",
        "The inflammation meter shows how badly the body is inflamed.",
        "If it gets too high, the body gets too weak to fight.",
        "Use the medicine button on immune cells to calm them."
    };

    void Start()
    {
        inflammationMeter.SetActive(false);
        initialPlayerPos = player.position;
        dialogueManager.SetDialogueLines(welcomeDialogue);
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(OnWelcomeEnd);
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

    void OnMovementComplete()
    {
        tutorialStep = 2;
        dialogueManager.SetDialogueLines(calmGammaDialogue);
        dialogueManager.StartDialogue();
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
        inflammationMeter.SetActive(true);
        dialogueManager.SetDialogueLines(inflammationDialogue);
        dialogueManager.StartDialogue();
        // Note: Immune calming mechanic not implemented yet
        // When ready, add detection here similar to gamma
    }
}
