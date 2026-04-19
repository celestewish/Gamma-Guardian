using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class InfoManager : MonoBehaviour
{
    List<Button> displayButtons;
    public GameObject bgCover;

    private bool isOn;

    void Awake()
    {
        bgCover = GameObject.FindGameObjectWithTag("InfoMask");
        if (bgCover != null && bgCover.activeSelf)
            bgCover.SetActive(false);

        displayButtons = new List<Button>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("InfoDisplay"))
        {
            Button temp = go.GetComponentInChildren<Button>();
            if (temp != null) displayButtons.Add(temp);
        }
        Debug.Log("Info Manager: " + displayButtons.Count + " buttons detected");

        isOn = false;
    }

    public void DisableButtons(int id)
    {
        foreach(Button bt in displayButtons)
        {
            if(bt.gameObject.transform.parent.transform.parent.gameObject.GetInstanceID() != id)
            {
                bt.interactable = false;
            }
        }
        bgCover.SetActive(true);
        isOn = true;
    }

    public void EnableButtons()
    {
        foreach (Button bt in displayButtons)
        {
            bt.interactable = true;
        }
        bgCover.SetActive(false);
        isOn = false;
    }
}
