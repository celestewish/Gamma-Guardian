using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InfoDisplay : MonoBehaviour
{
    public GameObject displayButton;
    //public TextMeshProUGUI buttonText;
    public TMP_Text buttonText;
    public bool fadeIn;
    public float displayRange;
    public Color tempColor;
    private bool displayOn = false;

    public GameObject displayText;
    private bool isOn = false;

    private float fixedDeltaTime;

    public Transform player;

    private Renderer objRenderer;
    private string sortingLayer;
    private Canvas canvas;
    private GameObject bgCover;
    private string bgSortingLayer;

    void Awake()
    {
        buttonText.text = "See Info";
        displayButton.SetActive(displayOn);
        displayText.SetActive(isOn);

        //tempColor = displayButton.GetComponent<UnityEngine.UI.Image>().color;

        objRenderer = transform.parent.gameObject.GetComponent<Renderer>(); //requires that the display object is the immediate child
        sortingLayer = objRenderer.sortingLayerName;

        canvas = GetComponent<Canvas>();

        bgCover = GameObject.FindGameObjectWithTag("InfoMask");
        if (bgCover != null)
        {
            bgSortingLayer = bgCover.GetComponent<Canvas>().sortingLayerName;
        }
        else
        {
            Debug.LogWarning("Could not find mask object");
        }
    }

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if(bgCover != null && bgCover.activeSelf)
            bgCover.SetActive(false);

        this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    void Update()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (displayOn && fadeIn)
        {
            float dist = Mathf.Min(distToPlayer, displayRange); //- .2f * displayRange
            tempColor = new Color(tempColor.r, tempColor.g, tempColor.g, (displayRange - dist)/displayRange);
        }

        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
    }

    void Display()
    {
        displayOn = !displayOn;
        displayButton.SetActive(displayOn);
        if (!displayOn && isOn)
            Switch();
    }

    public void Switch()
    {
        isOn = !isOn;
        displayText.SetActive(isOn);
        buttonText.text = (isOn) ? "Hide Info" : "See Info";

        if(bgCover != null)
            bgCover.SetActive(isOn);
        objRenderer.sortingLayerName = (isOn) ? bgSortingLayer : sortingLayer;
        canvas.sortingLayerName = (isOn) ? bgSortingLayer : sortingLayer;

        Time.timeScale = (isOn) ? 0f : 1f;
        AudioListener.pause = isOn;

        Debug.Log(objRenderer.gameObject.name+" --> new timescale: " + Time.timeScale);
    }
}
