using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VectorGraphics;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ComicPageSlider : MonoBehaviour
{
    [Header("Comic Page")]
    [SerializeField] private RectTransform comicImage;
    [SerializeField] private Vector2[] panelPositions;

    [SerializeField] private float slideDuration = 0.6f;
    [SerializeField] private Ease slideEase = Ease.InOutSine;

    [Header("Navigation")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button playButton;
    [SerializeField] private PlayerInput playerInput;

    [Header("Fade to Tutorial")]
    [SerializeField] private FadeController fadeController;

    [Header("Hint Flash")]
    [SerializeField] private float idleTimeBeforeFlash = 3f;
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private int flashLoops = 4;
    [SerializeField] private Color flashColor = Color.red;

    private int currentIndex = 0;
    private bool isSliding = false;
    private bool isPaused = false;

    private float idleTimer = 0f;
    private Image nextButtonImage;
    private Color nextButtonOriginalColor;
    private Tween flashTween;

    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextClicked);
            nextButtonImage = nextButton.GetComponent<Image>();
            if (nextButtonImage != null)
                nextButtonOriginalColor = nextButtonImage.color;
        }

        if (prevButton != null)
            prevButton.onClick.AddListener(OnPrevClicked);

        fadeController.FadeOut();

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        GoToPanel(currentIndex, instant: true);
        UpdateButtons();
        ResetIdleTimer();
    }

    private void Update()
    {
        if (isPaused) return;

        idleTimer += Time.unscaledDeltaTime;

        if (idleTimer >= idleTimeBeforeFlash)
        {
            StartFlashNextButton();
            // only start once per idle period
            idleTimer = -999f;
        }
    }

    private void ResetIdleTimer()
    {
        idleTimer = 0f;

        // stop any ongoing flash and restore color
        if (flashTween != null && flashTween.IsActive())
            flashTween.Kill();

        if (nextButtonImage != null)
            nextButtonImage.color = nextButtonOriginalColor;
    }

    private void StartFlashNextButton()
    {
        if (nextButtonImage == null) return;

        if (flashTween != null && flashTween.IsActive())
            flashTween.Kill();

        // flash between original color and flashColor
        flashTween = nextButtonImage
            .DOColor(flashColor, flashDuration)
            .SetLoops(flashLoops * 2, LoopType.Yoyo)   // there and back
            .OnComplete(() =>
            {
                nextButtonImage.color = nextButtonOriginalColor;
            });
    }

    private void GoToPanel(int index, bool instant = false)
    {
        if (comicImage == null || panelPositions == null || panelPositions.Length == 0)
            return;

        index = Mathf.Clamp(index, 0, panelPositions.Length - 1);
        Vector2 targetPos = panelPositions[index];

        if (instant)
        {
            comicImage.anchoredPosition = targetPos;
        }
        else
        {
            isSliding = true;
            comicImage.DOAnchorPos(targetPos, slideDuration)
                .SetEase(slideEase)
                .OnComplete(() => isSliding = false);
        }
    }

    private void UpdateButtons()
    {
        if (prevButton != null)
            prevButton.interactable = currentIndex > 0;

        if (nextButton != null)
            nextButton.interactable = true;
    }

    public void OnNextClicked()
    {
        if (isSliding) return;

        ResetIdleTimer();  // user interacted

        if (currentIndex >= panelPositions.Length - 1)
        {
            FadeToTutorial();
            return;
        }

        gameObject.SendMessage("NextLine");

        currentIndex++;
        GoToPanel(currentIndex, instant: false);
        UpdateButtons();
    }

    public void OnPrevClicked()
    {
        if (isSliding) return;
        if (currentIndex <= 0) return;

        ResetIdleTimer();  // user interacted

        gameObject.SendMessage("PrevLine");

        currentIndex--;
        GoToPanel(currentIndex, instant: false);
        UpdateButtons();
    }

    private void FadeToTutorial()
    {
        if (fadeController == null)
        {
            Debug.LogWarning("FadeController not set on ComicPageSlider.");
            return;
        }

        gameObject.SendMessage("ClearVO");

        Tween t = fadeController.FadeIn();
        if (t != null)
        {
            t.OnComplete(() =>
            {
                ProgressionManager.Instance.MarkIntroSeen();
                SceneManager.LoadScene("Tutorial");
            });
        }
        else
        {
            ProgressionManager.Instance.MarkIntroSeen();
            SceneManager.LoadScene("Tutorial");
            Debug.Log("Tutorial started (no fade tween).");
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        TogglePause();
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenu != null)
            pauseMenu.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;

        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap(isPaused ? "UI" : "Player");
            Debug.Log("Action map now: " + playerInput.currentActionMap.name);
        }
    }

    public void OnResumeButton()
    {
        if (!isPaused) return;
        TogglePause();
    }

    public void OnQuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}