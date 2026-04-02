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
    public float detectionDistance = 0.1f; // Small distance to detect movement

    private Vector3 initialPlayerPos;
    private HashSet<Vector2> movedDirections = new HashSet<Vector2>();
    private bool hasCalmedGamma = false;
    public int tutorialStep = 0;
    private bool hasDefeatedBacteria = false;

    private string[] welcomeDialogue = {
        "Welcome to the body, Guardian Explorer!",
        "Your goal is to help the immune system defeat the infection.",
        "This will allow us to defend the body."
    };

    private string[] moveDialogue = {
        "First, learn to move: try forwards, backwards, up, and down.",
        "Use W for up, A for left, D for right, and S for down.",
        "Try flying in a circle!"
    };

    private string[] immuneCell =
    {
        "Nice work! Let's learn about the immune cells now.",
        "This is an immune cell.",
        "Their job is to defend the body against pathogens.",
        "However the immune cells in this patient struggle to kill pathogens.",
        "To try and stop the infection, they call a cytokine.",
        "They are like little messengers who call for more immune cells."
    };

    private string[] calmGammaDialogue = {
        "This is a interferon gamma. A special type of cytokine that helps call the immune cells to attack infections,",
        "You see, the gammas are trying to help the body, but they get the immune cells too excited!",
        "When that happens, the cells get confused and start attacking the body instead!",
        "We have to make sure that there aren't too many cytokines in the body",
        "If there are too many, they will make the immune cells hyperactive and cause chaos!",
        "To calm the interferon gamma, fly up to the gamma and press the medicine button.",
        "The medicine button is the small square on the left."
    };

    private string[] bacteriaDialogue = {
        "Excellent work! Now let's tackle defeating the bacteria.",
        "The immune cells in this patient do not respond correctly to infection.",
        "They try super hard! But they can't defeat the bacteria on their own.",
        "We have to help them. Fly up to the bacteria and use the medicine button."
    };

    private string[] barDialogue1 =
    {
        "Perfect! Now that the bacteria is gone the body will be safe.",
        "If we don't defeat the bacteria, the patient won't be able to heal.",
        "Now, there's couple more things you need to know."
    };

    private string[] barDialogue2 =
    {
        "This flashing bar is the inflammation bar. It tells us how badly the body has inflammed.",
        "It will gradually increase and decrease as inflammation changes.",
        "Keep track of this bar, if it gets too full, it's game over.",
    };

    private string[] mapDialogue =
    {
        "This here is the map",
        "When there's bacteria on the map, they will show up as red dots.",
        "You can use the map to tell where enemies are"
    };

    private string[] endingDialogue =
    {
        "Here is a final tip for you Guardian Explorer.",
        "To get through the levels, you have to make sure to clear all the bacteria on the map.",
        "If all the bacteria are eliminated, then you can move on to the next section of the body.",
        "To help slow the inflammation down, keep cytokine levels at bay",
        "This will give you more time to defeat all of the bacteria",
        "Make sure not to take too long though! Time is limited, and you need to work quickly.",
        "Now Guardian Explorer, you have all you need to take on the infection and save the patient.",
        "Fly onwards and save them!"
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
        dialogueManager.onDialogueEnd.AddListener(MapTutorial);
        completionBar.SetActive(true);
        FlashUIColor(completionBar);
    }

    void MapTutorial()
    {
        dialogueManager.onDialogueEnd.RemoveAllListeners();
        dialogueManager.SetDialogueLines(mapDialogue);
        dialogueManager.StartDialogue();
        dialogueManager.onDialogueEnd.AddListener(OnTutorialComplete);
        map.SetActive(true);
        FlashUIColor(map);
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
        SceneManager.LoadScene("Level1");
    }
}
