using UnityEngine;

public class JoystickPlatformToggle : MonoBehaviour
{
    [SerializeField] private GameObject joystickRoot;

    private void Awake()
    {
        if (joystickRoot == null)
        {
            joystickRoot = gameObject;
        }

        joystickRoot.SetActive(Application.isMobilePlatform);
    }
}