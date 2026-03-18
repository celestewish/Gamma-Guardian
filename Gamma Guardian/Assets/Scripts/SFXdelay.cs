using UnityEngine;

public class SFXdelay : MonoBehaviour
{
    private AudioSource audioSource;

    public float delay; //default delay val
    public float offset; // random offset from delay
    private float length;
    private float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        length = audioSource.clip.length;
        //Debug.Log(audioSource.clip +"\n"+audioSource.clip.length);
        audioSource.loop = false;
        getNewTime();
    }

    // Update is called once per frame
    void Update()
    {
        if(timer <= 0f)
        {
            audioSource.Play();
            getNewTime();
        }
        timer -= Time.deltaTime;
    }

    void getNewTime()
    {
        timer = Random.Range(length+delay-offset, length+delay+offset);
    }
}
