using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class InfoManager : MonoBehaviour
{
    List<Button> displayButtons;
    //public GameObject infoMask; //mask that hides bg elements when an object is selected
    public GameObject bgCover; 
    [HideInInspector] public string bgSortingLayer;

    private Transform playerCam;
    private Vector3 targetDir; //direction to move camera
    private bool isCameraMoving=false;
    private float camMoveDist = 0;
    private float timeDelta;

    [HideInInspector] bool isOn;

    void Awake()
    {
        displayButtons = new List<Button>();

        timeDelta = Time.fixedDeltaTime; //time per frame

        isOn = false;
    }

    void Start()
    {
        playerCam = GameObject.FindGameObjectWithTag("MainCamera").transform;

        StartCoroutine(LateStart());
    }

    public bool IsDisplayUp() { return isOn; }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(.1f);
        Debug.Log("Info Manager: " + displayButtons.Count + " buttons detected");

        if (bgCover == null)
            bgCover = GameObject.FindGameObjectWithTag("InfoMask");

        bgSortingLayer = bgCover.GetComponent<Renderer>().sortingLayerName;
        if (bgCover != null && bgCover.activeSelf)
            bgCover.SetActive(false);
    }

    void Update()
    {
        if (playerCam == null)
        {
            playerCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }

        if (isCameraMoving)
        {
            if(camMoveDist >= targetDir.magnitude)
            {
                isCameraMoving = false;
                return;
            }

            //gives camera variable move speed
            if(camMoveDist < .6f * targetDir.magnitude)
            {
                playerCam.position += targetDir * 2.5f * timeDelta; //2.5
                camMoveDist += targetDir.magnitude * 2.5f * timeDelta;
            }
            else if (camMoveDist < .9f * targetDir.magnitude)
            {
                playerCam.position += targetDir * 1.5f * timeDelta; //1.75
                camMoveDist += targetDir.magnitude * 1.5f * timeDelta;
            }
            else
            {
                playerCam.position += targetDir * .5f * timeDelta; //.5
                camMoveDist += targetDir.magnitude * .5f * timeDelta;
            }
        }
    }

    public void DisableButtons(int id)
    {
        foreach(Button bt in displayButtons)
        {
            if (bt.gameObject.transform.parent.transform.parent.gameObject.GetInstanceID() != id)
            {
                bt.interactable = false;
            }
            else
            {
                //button that was clicked
                Vector3 temp1 = bt.GetComponent<RectTransform>().position;
                Vector3 temp2 = bt.GetComponent<RectTransform>().parent.Find("Image").GetComponent<RectTransform>().position;
                Vector3 temp = (temp1 + temp2) * .5f;
                targetDir = new Vector3(temp.x, temp.y, playerCam.position.z) - playerCam.position;
                Debug.Log("(I) Camera targetDirection -> " + targetDir);
            }
        }

        isOn = true;

        //enables background
        bgCover.SetActive(true);

        //reset
        camMoveDist = 0f;
        isCameraMoving = true;
    }

    public void EnableButtons()
    {
        foreach (Button bt in displayButtons)
        {
            bt.interactable = true;
        }

        isOn = false;

        //disables background
        bgCover.SetActive(false);

        //reverses cam shift
        targetDir = -targetDir;
        //Debug.Log("(II) Camera targetDirection -> " + targetDir);

        //reset
        camMoveDist = 0f;
        isCameraMoving = true;
    }

    public void AddButton(Button bttn)
    {
        displayButtons.Add(bttn);
    }

    public void RemoveFromInfoList(int id) //id of button
    {
        //Debug.Log("Looking for id " + id + " to remove");
        for (int i = 0;  i < displayButtons.Count; i++)
        {
            if(displayButtons[i].gameObject.GetInstanceID() == id)
            {
                //Debug.Log("Found id: " + id);
                displayButtons.RemoveAt(i);
                break;
            }
        }
    }
}
