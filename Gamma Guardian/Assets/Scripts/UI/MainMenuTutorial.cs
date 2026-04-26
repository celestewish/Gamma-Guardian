using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class MainMenuTutorial : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public string title;
        [TextArea(2, 4)] public string message;
        public RectTransform target;
    }

    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorialOverlay;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text stepText;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button skipButton;

    [Header("Highlight")]
    [SerializeField] private RectTransform highlightBox;
    [SerializeField] private UIPulseHighlight pulseHighlight;
    [SerializeField] private Vector2 highlightPadding = new Vector2(30f, 20f);

    [Header("Tutorial Steps")]
    [SerializeField] private TutorialStep[] steps;

    [Header("Settings")]
    [SerializeField] private bool showOnlyFirstTime = true;
    [SerializeField] private string playerPrefsKey = "MainMenuTutorialShown";

    private int currentStep = 0;
    private Canvas rootCanvas;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if (nextButton != null) nextButton.onClick.AddListener(NextStep);
        if (backButton != null) backButton.onClick.AddListener(PreviousStep);
        if (skipButton != null) skipButton.onClick.AddListener(SkipTutorial);
    }

    private void Start()
    {
        bool alreadyShown = PlayerPrefs.GetInt(playerPrefsKey, 0) == 1;

        if (showOnlyFirstTime && alreadyShown)
        {
            tutorialOverlay.SetActive(false);
            return;
        }

        StartTutorial();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetTutorialProgress();
            StartTutorial();
        }
    }

    public void StartTutorial()
    {
        if (steps == null || steps.Length == 0)
        {
            Debug.LogWarning("No tutorial steps assigned.");
            tutorialOverlay.SetActive(false);
            return;
        }

        tutorialOverlay.SetActive(true);
        currentStep = 0;
        UpdateStep();
    }

    public void NextStep()
    {
        currentStep++;

        if (currentStep >= steps.Length)
        {
            EndTutorial();
            return;
        }

        UpdateStep();
    }

    public void PreviousStep()
    {
        currentStep--;

        if (currentStep < 0)
            currentStep = 0;

        UpdateStep();
    }

    public void SkipTutorial()
    {
        EndTutorial();
    }

    private void EndTutorial()
    {
        ClearHighlight();
        tutorialOverlay.SetActive(false);
        PlayerPrefs.SetInt(playerPrefsKey, 1);
        PlayerPrefs.Save();
    }

    private void UpdateStep()
    {
        if (currentStep < 0 || currentStep >= steps.Length)
            return;

        ClearHighlight();

        TutorialStep step = steps[currentStep];

        if (titleText != null)
            titleText.text = step.title;

        if (messageText != null)
            messageText.text = step.message;

        if (stepText != null)
            stepText.text = "Step " + (currentStep + 1) + " of " + steps.Length;

        if (backButton != null)
            backButton.gameObject.SetActive(currentStep > 0);

        if (step.target != null)
            PositionHighlight(step.target);
    }

    private void PositionHighlight(RectTransform target)
    {
        if (highlightBox == null || target == null)
            return;

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector3 center = (corners[0] + corners[2]) / 2f;
        float width = corners[2].x - corners[0].x;
        float height = corners[2].y - corners[0].y;

        highlightBox.position = center;
        highlightBox.sizeDelta = new Vector2(
            width + highlightPadding.x,
            height + highlightPadding.y
        );

        highlightBox.localScale = Vector3.one;
        highlightBox.gameObject.SetActive(true);

        if (pulseHighlight != null)
        {
            pulseHighlight.SetBaseScale(Vector3.one);
            pulseHighlight.ResetPulse();
        }
    }

    private void ClearHighlight()
    {
        if (highlightBox == null)
            return;

        highlightBox.gameObject.SetActive(false);
        highlightBox.localScale = Vector3.one;

        if (pulseHighlight != null)
            pulseHighlight.ResetPulse();
    }

    public void ResetTutorialProgress()
    {
        PlayerPrefs.DeleteKey(playerPrefsKey);
        PlayerPrefs.Save();
    }
}