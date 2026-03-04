using UnityEngine;

public class PlayerAudioScript : MonoBehaviour
{
    [SerializeField]
    AudioSource audioSource;

    float pitchVal;

    void Update()
    {
        audioSource.pitch = pitchVal;
        //Debug.Log("thruster sound level: "+audioSource.pitch);
    }

    public void SetPitch(float val)
    {
        if (val > 2) val = 2;
        pitchVal = val + .1f; //makes it audible even when player is idle
    }
}
