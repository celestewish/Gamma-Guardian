using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ColorLock : MonoBehaviour
{
    public bool isLocked; //reference this when loading the unlockables
    //public Color disabledColor;

    void Start()
    {
        if (isLocked)
        {
            GetComponent<Image>().color = Color.white;
            GetComponent<Button>().interactable = false;
        }
        else
        {
            GetComponent<Button>().interactable = true;
        }
    }
}
