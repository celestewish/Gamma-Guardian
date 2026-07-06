using UnityEngine;

public class ComicVOScript : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip[] vo_clips;

    AudioSource VO_source;
    int clipIndex;

    void Start()
    {
        VO_source = GameObject.Find("AudioManager").transform.Find("VoiceOver").GetComponent<AudioSource>();
        clipIndex = 0;

        StartVO();
    }

    //Voice Over Handling
    public void StartVO()
    {
        VO_source.clip = vo_clips[clipIndex=0];
        VO_source.Play();
    }
    public void NextLine()
    {
        VO_source.Stop();

        if(clipIndex+1>=vo_clips.Length) return;

        VO_source.clip = vo_clips[++clipIndex];
        VO_source.Play();
    }
    public void PrevLine()
    {
        VO_source.Stop();

        if (clipIndex - 1 < 0) return;

        VO_source.clip = vo_clips[--clipIndex];
        VO_source.Play();
    }
    public void ClearVO()
    {
        VO_source.Stop();
        VO_source.clip = null;
        clipIndex = 0;
    }
}
