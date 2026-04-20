using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class EndingScene : MonoBehaviour
{
    [Header("Player")]
    public GameObject player;

    [Header("Camera")]
    public new Camera camera;

    [Header("Background")]
    public GameObject background;
    public float backgroundScrollSpeed = 2f;

    [Header("Dialogue")]
    public DialogueManager dialogueManager;

    [Header("Glossary Icon")]
    public GameObject glossaryIcon;

    [Header("Flash Effect")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.5f;
    public int flashLoops = 3;

    [Header("Settings")]
    public float flySpeed = 5f;
    public float hoverTime = 1f;
    public Vector2 targetHoverPos;

    [Header("Fade Image")]
    public Image fadeImage;

    [Header("Float Settings")]
    public float floatAmplitude = 0.5f;
    public float floatSpeed = 2f;
    public float floatPhase = 0f;


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
        dialogueManager.onDialogueEnd.AddListener(OnDialogueComplete);

        StartCoroutine(EndingSequence());
    }

    void Update()
    {
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            pos.y = targetHoverPos.y + Mathf.Sin(Time.time * floatSpeed + floatPhase) * floatAmplitude;
            player.transform.position = pos;
        }

        if (background != null)
        {
            float offset = Mathf.Repeat(Time.time * backgroundScrollSpeed, 1f);
            ScrollBackground(offset);
        }
    }


    IEnumerator EndingSequence()
    {
        dialogueManager.StartDialogue();

        while (Vector2.Distance(player.transform.position, targetHoverPos) > 0.1f)
        {
            player.transform.position = Vector2.MoveTowards(player.transform.position, targetHoverPos, flySpeed * Time.deltaTime);
            yield return null;
        }

        float hoverTimer = 0f;
        while (hoverTimer < hoverTime)
        {
            hoverTimer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitUntil(() => dialogueManager.currentLineIndex == 1);

        glossaryIcon.SetActive(true);
        FlashUIColor(glossaryIcon);

        yield return new WaitUntil(() => dialogueEnded);

        Vector2 exitPos = new Vector2(30f, player.transform.position.y);
        while (Vector2.Distance(player.transform.position, exitPos) > 0.1f)
        {
            player.transform.position = Vector2.MoveTowards(player.transform.position, exitPos, flySpeed * 1.5f * Time.deltaTime);
            yield return null;
        }

        StartCoroutine(FadeAndLoad());
    }

    void ScrollBackground(float xOffset)
    {
        if (background == null) return;

        Transform bgTransform = background.transform;
        bgTransform.Translate(Vector3.left * backgroundScrollSpeed * Time.deltaTime, Space.World);

        SpriteRenderer[] srs = background.GetComponentsInChildren<SpriteRenderer>();
        float totalWidth = 0f;
        foreach (SpriteRenderer sr in srs) totalWidth += sr.bounds.size.x;

        float leftmostX = float.MaxValue;
        foreach (SpriteRenderer sr in srs)
        {
            leftmostX = Mathf.Min(leftmostX, sr.bounds.min.x);
        }
        if (leftmostX < -24.2f)
        {
            bgTransform.position += new Vector3(20, 0, 0);
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

