using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{

    [SerializeField] private float speedMult;
    [SerializeField] private float maxSpeed;

    [SerializeField] private FalloffType foType;
    [SerializeField] private float foMult;
    private float foVal;

    private Vector2 move;
    private Rigidbody2D rb;
    private PlayerAudioScript playerAudio;

    void Start()
    {
        if (speedMult <= 0) speedMult = 5f;
        move = new Vector2(0, 0);
        rb = GetComponent<Rigidbody2D>();
        playerAudio = GetComponent<PlayerAudioScript>();

        if (foMult > .1f) foMult = .1f;
        foVal = foMult;
    }

    void OnMove(InputValue ip)
    {
        move = ip.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (move.magnitude > 0)
        {
            rb.AddForce(move.normalized * speedMult);
            if (rb.linearVelocity.magnitude > maxSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
        else if (rb.linearVelocity.magnitude > 0)
        {
            if (rb.linearVelocity.magnitude > 1f)
            {
                if ((int)foType == 0) rb.linearVelocity = rb.linearVelocity * (1 - foVal); //exponential fall-off
                else rb.linearVelocity = rb.linearVelocity - (foVal * maxSpeed * rb.linearVelocity.normalized); //linear fall-off
            }
            else
                rb.linearVelocity = new Vector2(0, 0);
        }

        playerAudio.SetPitch(1.4f * rb.linearVelocity.magnitude / maxSpeed); //calls SetPitch with values from 0 - 1.4
        //Debug.Log("dir: " + rb.linearVelocity.magnitude);
    }

    //enum FalloffLevel
    //{
    //    _0 = 0,
    //    _1 = 1,
    //    _2 = 2,
    //    _3 = 3,
    //    _4 = 4
    //};

    enum FalloffType
    {
        Exponential, //speed fall-off exponentially decreases
        Linear //speed fall-off decreases linearly (portion of maxspeed / time)
    };

    public void DeactivateCytokine()
    {
        GameObject[] cytokines = GameObject.FindGameObjectsWithTag("Cytokines");
        GameObject closestCytokine = null;
        float closestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (GameObject cytokine in cytokines)
        {
            float distance = Vector3.Distance(currentPosition, cytokine.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCytokine = cytokine;
            }
        }
        if (closestCytokine != null && closestDistance < 2f)
        {
            CytokinesScript cytokineScript = closestCytokine.GetComponent<CytokinesScript>();
            if (cytokineScript != null)
            {
                cytokineScript.deactivated = true;
                cytokineScript.Deactivate();
            }
        }
    }
}
