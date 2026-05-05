using UnityEngine;

public class ExitGameScript : MonoBehaviour
{
    public GameObject exitMenu;

    void Start()
    {
        exitMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        exitMenu.SetActive(!exitMenu.activeInHierarchy);
    }
}
