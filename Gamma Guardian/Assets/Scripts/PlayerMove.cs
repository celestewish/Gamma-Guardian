using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public float speed;
    private Vector2 move;
    private Rigidbody2D rb;

    void Start()
    {
        if (speed <= 0) speed = 5f;
        move = new Vector2(0, 0);
        rb = GetComponent<Rigidbody2D>();
    }

    void OnMove(InputValue ip)
    {
        move = ip.Get<Vector2>();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + speed * Time.deltaTime * move);
    }
}
