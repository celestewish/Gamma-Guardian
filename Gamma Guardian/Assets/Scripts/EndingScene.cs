using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class EndingScene : MonoBehaviour
{
    [Header("Player")]
    public GameObject player; // Player GameObject with Rigidbody2D or Transform mover

    [Header("Camera")]
    public new Camera camera; // Main camera

    [Header("Background")]
    public GameObject background; // Parallax background parent or single object
    public float backgroundScrollSpeed = 2f;

    [Header("Dialogue")]
    public DialogueManager dialogueManager;

    [Header("Glossary Icon")]
    public GameObject glossaryIcon; // Icon that appears and flashes red

    [Header("Flash Effect")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.5f;
    public int flashLoops = 3;

    [Header("Settings")]
    public float flySpeed = 5f;
    public float hoverTime = 1f; // Time hovering before dialogue starts
    public Vector2 targetHoverPos; // Slightly left of center (set in inspector, e.g., (-2f, 0f))

    [Header("Fade Image")]
    public Image fadeImage;

    private Vector3 startPos;
    private bool dialogueEnded = false;
    private string[] lines = {
            "Congratulations! You've stopped the infection and prevented the body from inflaming too much.",
            "Check out the glossary post-game to learn more about the awesome things you just did!"
        };

    void Start()
    {
        startPos = player.transform.position;
        glossaryIcon.SetActive(false);
        fadeImage.gameObject.SetActive(false);
        dialogueManager.SetDialogueLines(lines);

        // Subscribe to dialogue end
        dialogueManager.onDialogueEnd.AddListener(OnDialogueComplete);

        StartCoroutine(EndingSequence());
    }

    IEnumerator EndingSequence()
    {
        // Phase 1: Fly in from left to hover position
        while (Vector2.Distance(player.transform.position, targetHoverPos) > 0.1f)
        {
            player.transform.position = Vector2.MoveTowards(player.transform.position, targetHoverPos, flySpeed * Time.deltaTime);
            yield return null;
        }

        // Phase 2: Hover and scroll background
        float hoverTimer = 0f;
        while (hoverTimer < hoverTime)
        {
            // Scroll background to simulate forward movement (adjust child renderers or single bg)
            if (background != null)
            {
                float offset = Mathf.Repeat(Time.time * backgroundScrollSpeed, 1f);
                // Assuming background has Renderer with offset, or children with Parallax script
                // Example for single bg: background.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(offset, 0);
                ScrollBackground(offset);
            }
            hoverTimer += Time.deltaTime;
            yield return null;
        }

        // Phase 3: Start dialogue
        dialogueManager.StartDialogue();
        yield return new WaitUntil(() => dialogueManager.currentLineIndex == 1); // Wait for second line

        // Show and flash icon during last line
        glossaryIcon.SetActive(true);
        FlashUIColor(glossaryIcon);

        // Wait for dialogue end
        yield return new WaitUntil(() => dialogueEnded);

        // Phase 4: Zoom to right off-screen
        Vector2 exitPos = new Vector2(30f, player.transform.position.y); // Adjust right edge
        while (Vector2.Distance(player.transform.position, exitPos) > 0.1f)
        {
            player.transform.position = Vector2.MoveTowards(player.transform.position, exitPos, flySpeed * 1.5f * Time.deltaTime);
            yield return null;
        }

        // Phase 5: Fade to black and load menu
        StartCoroutine(FadeAndLoad());
    }

    void ScrollBackground(float xOffset)
    {
        // Implement based on your bg setup, e.g., for tiled sprites
        Renderer rend = background.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.mainTextureOffset = new Vector2(xOffset, 0);
        }
        // Or loop through children:
        // foreach (Transform child in background.transform) { ... }
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

    void OnDialogueComplete()
    {
        dialogueEnded = true;
        glossaryIcon.gameObject.SetActive(false);
    }

    IEnumerator FadeAndLoad()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            float fadeTime = 1f;
            for (float alpha = 0f; alpha <= 1f; alpha += Time.deltaTime / fadeTime)
            {
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }
        SceneManager.LoadScene("MainMenu");
    }
}

