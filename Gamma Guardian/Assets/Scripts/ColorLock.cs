using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ColorLock : MonoBehaviour
{
    public bool isLocked;
    public Color disabledColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (isLocked)
        {
            GetComponent<Image>().color = disabledColor;
            GetComponent<Button>().interactable = false;
        }
        else
        {
            GetComponent<Button>().interactable = true;
        }
    }
}
