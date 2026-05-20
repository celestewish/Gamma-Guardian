using UnityEngine;

public class RbDisplayTest : MonoBehaviour
{
    Rigidbody2D rb;
    bool has;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb=GetComponent<Rigidbody2D>();
        has=(rb!=null);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (has)
            Debug.DrawRay(transform.position, rb.linearVelocity, Color.red);
    }
}
