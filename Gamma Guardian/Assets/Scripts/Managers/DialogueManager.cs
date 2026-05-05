using System; // For Action
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public Image dialoguebox;
    public Button nextButton;
    public Button backButton;
    public TMP_Text dialogueText;
    public AudioSource sfxSource; // Assign typing sound clip here

    [Header("Settings")]
    public float charInterval = 0.05f;
    public List<string> dialogueLines = new List<string>();

    [Header("Events")]
    public UnityEvent onDialogueEnd = new UnityEvent(); // Added UnityEvent

    public int currentLineIndex = 0;
    private Coroutine typeCoroutine;
    private bool isTyping = false;

    void Start()
    {
        nextButton.onClick.AddListener(OnNextPressed);
        if (backButton != null)
            backButton.onClick.AddListener(OnBackPressed);
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.text = "";
        if (dialogueLines.Count > 0) StartDialogue();
    }

    // Added public method for setting lines externally
    public void SetDialogueLines(string[] lines)
    {
        dialogueLines.Clear();
        dialogueLines.AddRange(lines);
        currentLineIndex = 0;
        dialoguebox.gameObject.SetActive(true); // Show box for new dialogue
        dialogueText.maxVisibleCharacters = 0;
    }

    // Added public method to start dialogue externally
    public void StartDialogue()
    {
        // Always stop any current typing first
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        if (currentLineIndex < dialogueLines.Count)
        {
            // Set text once here
            dialogueText.text = dialogueLines[currentLineIndex];
            dialogueText.maxVisibleCharacters = 0;

            // Now start typing
            typeCoroutine = StartCoroutine(TypeText(dialogueLines[currentLineIndex]));
        }
    }

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;

        // Text is already set in StartDialogue
        dialogueText.ForceMeshUpdate();
        int totalVisibleChars = dialogueText.textInfo.characterCount;
        int visibleCount = 0;

        while (visibleCount < totalVisibleChars)
        {
            visibleCount++;
            dialogueText.maxVisibleCharacters = visibleCount;

            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(sfxSource.clip);
            }

            yield return new WaitForSeconds(charInterval);
        }

        isTyping = false;
        typeCoroutine = null;
    }

    void ShowAllCurrentText()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        dialogueText.ForceMeshUpdate();
        dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        isTyping = false;
    }

    void OnNextPressed()
    {
        if (isTyping)
        {
            ShowAllCurrentText();
            return;
        }

        if (currentLineIndex < dialogueLines.Count - 1)
        {
            currentLineIndex++;
            StartDialogue();
        }
        else
        {
            EndDialogue();
        }
    }

    void OnBackPressed()
    {
        if (isTyping)
        {
            ShowAllCurrentText();
            return;
        }

        if (currentLineIndex > 0)
        {
            currentLineIndex--;
            StartDialogue();
        }
    }

    void EndDialogue()
    {
        dialoguebox.gameObject.SetActive(false);
        onDialogueEnd?.Invoke(); // Notify listeners like TutorialManager
    }
}
