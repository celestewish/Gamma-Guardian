using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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
    public TrailRenderer playerTrail;
    public PlayerAbilitySFX playerAbilitySFX;

    public AudioClip bumpSound;
    [SerializeField] private AudioSource sfxAudio;

    [SerializeField] private SpriteRenderer playerSprite;     // Main player sprite
    [SerializeField] private SpriteRenderer actionSprite;     // Action sprite (child)
    [SerializeField] private float actionShowTime = 1f;       // Seconds to show action
    private bool isFacingRight = true;

    void Start()
    {
        if (actionSprite != null) actionSprite.gameObject.SetActive(false);

        if (speedMult <= 0) speedMult = 5f;
        move = new Vector2(0, 0);
        rb = GetComponent<Rigidbody2D>();
        playerAudio = GetComponent<PlayerAudioScript>();

        if (foMult > .1f) foMult = .1f;
        foVal = foMult;

        //Time.fixedDeltaTime = 0.02f;
        Application.targetFrameRate = 60;
    }

    void OnMove(InputValue ip)
    {
        Vector2 newMove = ip.Get<Vector2>();
        /*bool isMoving = newMove.magnitude > 0.3f; // threshold to count as intentional

        if (isMoving && !wasMoving) // only fires on rising edge
        {
            Vector2 newDir = newMove.normalized;
            bool sameDirection = Vector2.Dot(newDir, lastTapDir.normalized) > 0.8f;
            bool withinWindow = (Time.time - lastTapTime) < doubleTapWindow;

            if (sameDirection && withinWindow)
            {
                dashEndTime = Time.time + dashDuration;
                dashDirection = newDir;
            }

            lastTapTime = Time.time;
            lastTapDir = newDir;
        }

        wasMoving = isMoving;
        */
        move = newMove;
    }

    private bool wasMoving = false;

    [SerializeField] private float dashSpeed = 2.0f;    // multiplier of maxSpeed
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float doubleTapWindow = 0.25f; // seconds between taps to count

    private bool isDashing = false;
    private Vector2 lastMoveDir = Vector2.zero;
    private float lastTapTime = -1f;
    private Vector2 lastTapDir = Vector2.zero;

    private float dashEndTime = -1f;
    private Vector2 dashDirection;

    public void OnDashButton()
    {
        if (move.magnitude > 0.1f && Time.time >= dashEndTime)
        {
            dashEndTime = Time.time + dashDuration;
            dashDirection = move.normalized;
        }
    }




    void OnAbility(InputValue value)
    {
        if (value.isPressed)
        {
            PlayerAction();
            playerAbilitySFX.OnButtonClick();
        }
    }

    private void Update()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
            playerTrail.emitting = true;
        else
            playerTrail.emitting = false;

        if (move.x != 0)
        {
            bool newFacingRight = move.x > 0;
            if (newFacingRight != isFacingRight)
            {
                isFacingRight = newFacingRight;
                if (playerSprite != null) playerSprite.flipX = !isFacingRight;
                if (actionSprite != null) actionSprite.flipX = !isFacingRight;
            }
        }
        bool isDashing = Time.time < dashEndTime;
        if (isDashing)
            rb.linearVelocity = dashDirection * maxSpeed * dashSpeed;
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
    }

    void FixedUpdate()
    {
        playerAudio.SetPitch(1.4f * rb.linearVelocity.magnitude / maxSpeed); //calls SetPitch with values from 0 - 1.4
        //Debug.Log("player speed: " + rb.linearVelocity.magnitude);
    }

    public void PlayerAction()
    {
        if (actionSprite != null)
        {
            actionSprite.flipX = !isFacingRight;
            StartCoroutine(ShowActionCoroutine());
        }
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
        else
        {
            GameObject[] bacterias = GameObject.FindGameObjectsWithTag("Bacteria");
            GameObject closestBacteria = null;
            closestDistance = Mathf.Infinity;
            foreach (GameObject bacteria in bacterias)
            {
                float distance = Vector3.Distance(currentPosition, bacteria.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBacteria = bacteria;
                }
            }
            if (closestBacteria != null && closestDistance < 2f)
            {
                BacteriaAI bacteriaScript = closestBacteria.GetComponent<BacteriaAI>();
                if (bacteriaScript != null)
                {
                    bacteriaScript.Die();
                }
            }
        }

        
    }
    private IEnumerator ShowActionCoroutine()
    {
        if (actionSprite != null)
        {
            actionSprite.gameObject.SetActive(true);
            playerSprite.enabled = false;
            yield return new WaitForSeconds(actionShowTime);
            actionSprite.gameObject.SetActive(false);
            playerSprite.enabled = true;
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        Debug.Log("hit");
        if (coll.gameObject.tag == "Wall")
        {
            Debug.Log("wall");
            sfxAudio.PlayOneShot(bumpSound);
        }
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
}
