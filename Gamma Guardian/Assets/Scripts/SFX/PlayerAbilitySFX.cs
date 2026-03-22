using Unity.VisualScripting;
using UnityEngine;

public class PlayerAbilitySFX : MonoBehaviour
{
    public GameObject playerAbility;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnButtonClick()
    {
        if (playerAbility != null)
        {
            Instantiate(playerAbility,transform.position, transform.rotation);
        }
    }
}
