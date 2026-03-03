using UnityEngine;

public class PlayerAudioScript : MonoBehaviour
{
    [SerializeField]
    AudioSource audio;

    private float cooldown;
    int dir;

    void Start()
    {
        cooldown = 1f;
        dir=1;
    }

    // Update is called once per frame
    void Update()
    {
        if(cooldown <= 0)
        {
            dir = -dir;
            cooldown = 1;
        }
        cooldown-=Time.deltaTime;
        audio.pitch = audio.pitch + dir*Time.deltaTime;
    }
}
