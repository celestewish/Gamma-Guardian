using Unity.VisualScripting;
using UnityEngine;

public class PlayerAbilitySFX : MonoBehaviour
{
    public GameObject playerAbility;
    public AudioSource audioSource;
    public AudioClip audioClip;

    public void OnButtonClick()
    {
        if (playerAbility != null)
        {
            Instantiate(playerAbility,transform.position, transform.rotation);
            audioSource.PlayOneShot(audioClip);
        }
    }
}
