using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
//using System.Collections.Generic;

public class InfoDisplay : MonoBehaviour
{
    public GameObject displayButton; //button reference
    public TMP_Text buttonText;

    public bool fadeIn; // yes/no for text fading as player moves away, currently unused
    public float displayRange; //range cutoff for fading effect
    public Color tempColor; //color used in fading effect

    private bool displayOn = false;
    public GameObject displayText; //info textbox
    private bool isOn;

    private float fixedDeltaTime; //time per frame

    public Transform player;

    private Renderer objRenderer;
    private string sortingLayer;
    private Canvas canvas;

    GameObject manager; //game manager reference
    string bgSortingLayer; //sorting layer for background image

    void Awake()
    {
        buttonText.text = "See Info";

        //tempColor = displayButton.GetComponent<UnityEngine.UI.Image>().color;

        objRenderer = transform.parent.gameObject.GetComponent<Renderer>(); //requires that the display object is the immediate child
        sortingLayer = objRenderer.sortingLayerName;

        canvas = GetComponent<Canvas>();
    }

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        this.fixedDeltaTime = Time.fixedDeltaTime;

        manager = GameObject.Find("Game Manager"); //adds button to manager\

        displayOn = manager.GetComponent<GameManager>().IsDisplayOn();
        displayButton.SetActive(displayOn);
        //if (manager.GetComponent<GameManager>().IsDisplayOn())
        //    displayButton.SetActive(isOn = true);
        //else
        //    displayButton.SetActive(isOn = false);

        displayText.SetActive(false);
        //Debug.Log(transform.parent.gameObject.name + ": " + transform.parent.gameObject.GetInstanceID());

        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(.25f);

        this.bgSortingLayer = manager.GetComponent<InfoManager>().bgSortingLayer;

        manager.GetComponent<InfoManager>().AddButton(displayButton.GetComponent<Button>());
    }

    void Update()
    {
        //float distToPlayer = Vector2.Distance(transform.position, player.position);
        //if (displayOn && fadeIn)
        //{
        //    float dist = Mathf.Min(distToPlayer, displayRange); //- .2f * displayRange
        //    tempColor = new Color(tempColor.r, tempColor.g, tempColor.g, (displayRange - dist)/displayRange);
        //}

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

        if (isOn)
            manager.GetComponent<InfoManager>().DisableButtons(transform.parent.gameObject.GetInstanceID());
        else 
            manager.SendMessage("EnableButtons");

        //sorting layer toggles
        objRenderer.sortingLayerName = (isOn) ? bgSortingLayer : sortingLayer;
        canvas.sortingLayerName = (isOn) ? bgSortingLayer : sortingLayer;

        Time.timeScale = (isOn) ? 0f : 1f;
        AudioListener.pause = isOn;
    }

    public void RemoveMeFromInfo()
    {
        manager.SendMessage("RemoveFromInfoList", displayButton.GetInstanceID());
    }
}
