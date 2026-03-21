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
        if (currentLineIndex < dialogueLines.Count)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
            typeCoroutine = StartCoroutine(TypeText(dialogueLines[currentLineIndex]));
        }
    }

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        dialogueText.maxVisibleCharacters = 0;

        foreach (char c in fullText)
        {
            // Play sound effect
            if (sfxSource != null && c != ' ')
            {
                sfxSource.PlayOneShot(sfxSource.clip);
            }

            dialogueText.maxVisibleCharacters++;
            yield return new WaitForSeconds(charInterval);
        }

        dialogueText.maxVisibleCharacters = fullText.Length;
        isTyping = false;
    }

    void OnNextPressed()
    {
        if (isTyping)
        {
            // Show full text instantly
            if (typeCoroutine != null) StopCoroutine(typeCoroutine);
            dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
            isTyping = false;
        }
        else if (currentLineIndex < dialogueLines.Count - 1)
        {
            // Next line
            currentLineIndex++;
            StartDialogue();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialoguebox.gameObject.SetActive(false);
        onDialogueEnd?.Invoke(); // Notify listeners like TutorialManager
    }
}
