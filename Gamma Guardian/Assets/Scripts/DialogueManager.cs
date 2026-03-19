using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private int currentLineIndex = 0;
    private Coroutine typeCoroutine;
    private bool isTyping = false;

    void Start()
    {
        nextButton.onClick.AddListener(OnNextPressed);
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.text = "";
        if (dialogueLines.Count > 0) StartDialogue();
    }

    void StartDialogue()
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
            StopCoroutine(typeCoroutine);
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
    }
}
