using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VectorGraphics;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ComicPageSlider : MonoBehaviour
{
    [Header("Comic Page")]
    [SerializeField] private RectTransform comicImage;   // The full page image

    [Tooltip("Anchored positions for each panel view (x,y of the image).")]
    [SerializeField] private Vector2[] panelPositions;   // One per panel

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

    private int currentIndex = 0;
    private bool isSliding = false;
    private bool isPaused = false;

    private void Start()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);
        if (prevButton != null)
            prevButton.onClick.AddListener(OnPrevClicked);
        fadeController.FadeOut();
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        GoToPanel(currentIndex, instant: true);
        UpdateButtons();
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

        // Keep Next enabled so clicking at last index can trigger tutorial
        if (nextButton != null)
            nextButton.interactable = true;
    }

    public void OnNextClicked()
    {
        if (isSliding) return;

        if (currentIndex >= panelPositions.Length - 1)
        {
            FadeToTutorial();
            return;
        }

        currentIndex++;
        GoToPanel(currentIndex, instant: false);
        UpdateButtons();
    }

    public void OnPrevClicked()
    {
        if (isSliding) return;
        if (currentIndex <= 0) return;

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

        Tween t = fadeController.FadeIn();
        if (t != null)
        {
            t.OnComplete(() =>
            {
                ProgressionManager.Instance.MarkIntroSeen();
                SceneManager.LoadScene("Tutorial");
                SceneManager.LoadScene("Tutorial");
            });
        }
        else
        {
            ProgressionManager.Instance.MarkIntroSeen();
            SceneManager.LoadScene("Tutorial");
            SceneManager.LoadScene("Tutorial");
            Debug.Log("Tutorial started (no fade tween).");
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        // Only react on performed, not started/canceled
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

    // Hook these to your pause menu buttons
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