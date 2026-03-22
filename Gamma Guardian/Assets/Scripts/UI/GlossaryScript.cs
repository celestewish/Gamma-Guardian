using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;              // Only if you use TextMeshPro
using UnityEngine.SceneManagement;

[System.Serializable]
public class GlossaryEntry
{
    public Sprite image;
    public string title;
    [TextArea(3, 6)]
    public string description;
}

public class GlossaryScript : MonoBehaviour
{
    [Header("UI References")]
    public Image imageSlot;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    public Button nextButton;
    public Button previousButton;
    public Button closeButton;

    [Header("Data")]
    public List<GlossaryEntry> entries = new List<GlossaryEntry>();

    private int currentIndex = 0;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip navSfx;
    public AudioClip closeSfx;

    void Awake()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(NextEntry);

        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousEntry);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseGlossary);
    }

    void Start()
    {
        if (entries.Count > 0)
        {
            currentIndex = 0;
            RefreshUI();
        }
        else
        {
            if (imageSlot != null) imageSlot.sprite = null;
            if (titleText != null) titleText.text = "";
            if (descriptionText != null) descriptionText.text = "";
        }
    }

    void RefreshUI()
    {
        if (entries.Count == 0 || currentIndex < 0 || currentIndex >= entries.Count)
            return;

        GlossaryEntry entry = entries[currentIndex];

        if (imageSlot != null)
            imageSlot.sprite = entry.image;

        if (titleText != null)
            titleText.text = entry.title;

        if (descriptionText != null)
            descriptionText.text = entry.description;

        //Enable/disable navigation buttons at ends
        if (previousButton != null)
            previousButton.interactable = currentIndex > 0;

        if (nextButton != null)
            nextButton.interactable = currentIndex < entries.Count - 1;
    }

    public void NextEntry()
    {
        if (entries.Count == 0)
            return;

        currentIndex = (currentIndex + 1) % entries.Count;
        PlaySound(true);
        RefreshUI();
    }

    public void PreviousEntry()
    {
        if (entries.Count == 0)
            return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = entries.Count - 1;
        PlaySound(true);
        RefreshUI();
    }

    public void CloseGlossary()
    {
        // Load MainMenu scene
        PlaySound(false);
        SceneManager.LoadScene("MainMenu");
    }

    void PlaySound(bool open)
    {
        AudioClip clickSfx = (open) ? navSfx : closeSfx;

        if (uiAudioSource != null && clickSfx != null)
        {
            uiAudioSource.PlayOneShot(clickSfx);
        }
    }
}
