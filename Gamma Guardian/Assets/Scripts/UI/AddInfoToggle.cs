using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AddInfoToggle : MonoBehaviour
{
    void Awake()
    {
        Button button = GetComponent<Button>();
        GameManager gm = GameObject.Find("Game Manager").GetComponent<GameManager>();
        button.onClick.AddListener(gm.SwitchMode);
    }
}
