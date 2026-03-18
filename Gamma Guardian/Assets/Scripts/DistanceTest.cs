using UnityEngine;

public class DistanceTest : MonoBehaviour
{
    public Transform obj1;
    public Transform obj2;
    public bool isOn;

    // Update is called once per frame
    void Update()
    {
        if (!isOn) return;
        Debug.Log("distance: "+ Vector2.Distance(obj1.position, obj2.position));
        Debug.DrawLine(obj1.position, obj2.position, Color.green);
    }
}
