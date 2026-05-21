using UnityEngine;

public class SpriteMoveTest : MonoBehaviour
{
    public Transform testObj;

    public Transform spot;

    public float duration;
    private float uptime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float temp = (uptime%duration)/duration;
        Vector3 loc = spot.position*temp + transform.position*(1-temp);
        testObj.position = loc;

        uptime += Time.deltaTime;
    }
}
