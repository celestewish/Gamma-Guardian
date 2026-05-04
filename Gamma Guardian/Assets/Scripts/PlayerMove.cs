using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMove : MonoBehaviour
{

    [SerializeField] private float speedMult;
    [SerializeField] private float maxSpeed;
    public bool canMove = false; // Starts locked; LevelManager will unlock it

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

    [SerializeField] private Color afterimageColor = new Color(0.4f, 0.8f, 1f, 0.5f);
    [SerializeField] private float afterimageFadeDuration = 0.2f;


    void Start()
    {
        if (actionSprite != null) actionSprite.gameObject.SetActive(false);

        if (speedMult <= 0) speedMult = 5f;
        move = new Vector2(0, 0);
        rb = GetComponent<Rigidbody2D>();
        playerAudio = GetComponent<PlayerAudioScript>();

        if (foMult > .1f) foMult = .1f;
        foVal = foMult;

        Application.targetFrameRate = 60;

        // If no LevelManager is present (e.g. Tutorial), unlock movement immediately
        GameManager lm = FindFirstObjectByType<GameManager>();
        if (lm == null || !lm.levelRunning)
            canMove = true;
    }
    void OnMove(InputValue ip)
    {
        if (!canMove)
        {
            move = Vector2.zero;
            return;
        }
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

    [SerializeField] private float dashImpulse = 50f;   // Impulse force magnitude
    [SerializeField] private float dashCooldown = 0f; // Seconds before dashing again
    private float lastDashTime = -999f;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private float dashPitchMin = 0.9f;
    [SerializeField] private float dashPitchMax = 1.15f;

    public void OnDashButton()
    {
        if (!canMove) return;
        if (move.magnitude > 0.1f && Time.time >= lastDashTime + dashCooldown)
        {
            lastDashTime = Time.time;
            Vector2 dashDir = move.normalized;

            if (rb.linearVelocity.magnitude > maxSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

            rb.AddForce(dashDir * dashImpulse, ForceMode2D.Impulse);

            // Audio: random pitch so repeated dashes don't sound robotic
            if (dashSound != null)
            {
                sfxAudio.pitch = Random.Range(dashPitchMin, dashPitchMax);
                sfxAudio.PlayOneShot(dashSound);
            }

            // Visuals: trail flare
            StartCoroutine(TrailFlare());
            StartCoroutine(SpawnAfterimage());
        }
    }

    private IEnumerator TrailFlare()
    {
        float originalWidth = playerTrail.startWidth;
        playerTrail.startWidth = originalWidth * 3f;

        float elapsed = 0f;
        float fadeDuration = 0.15f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            playerTrail.startWidth = Mathf.Lerp(originalWidth * 3f, originalWidth, elapsed / fadeDuration);
            yield return null;
        }

        playerTrail.startWidth = originalWidth;
    }

    private IEnumerator SpawnAfterimage()
    {
        // Create a ghost object at the player's current position/rotation
        GameObject ghost = new GameObject("DashAfterimage");
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;
        ghost.transform.localScale = transform.localScale;

        // Copy the sprite renderer
        SpriteRenderer ghostRenderer = ghost.AddComponent<SpriteRenderer>();
        ghostRenderer.sprite = playerSprite.sprite;
        ghostRenderer.flipX = playerSprite.flipX;
        ghostRenderer.sortingLayerName = playerSprite.sortingLayerName;
        ghostRenderer.sortingOrder = playerSprite.sortingOrder - 1; // render behind player
        ghostRenderer.color = afterimageColor;

        // Fade out
        float elapsed = 0f;
        while (elapsed < afterimageFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(afterimageColor.a, 0f, elapsed / afterimageFadeDuration);
            ghostRenderer.color = new Color(afterimageColor.r, afterimageColor.g, afterimageColor.b, alpha);
            yield return null;
        }

        Destroy(ghost);
    }


    void OnAbility(InputValue value)
    {
        if (!canMove) return;
        if (value.isPressed)
        {
            PlayerAction();
            playerAbilitySFX.OnButtonClick();
        }
    }

    private void Update()
    {
        if (!canMove)
        {
            move = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            playerTrail.emitting = false;
            return; // Skip all movement logic below
        }
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
            // Try normal cytokine first
            CytokinesScript cytokineScript = closestCytokine.GetComponent<CytokinesScript>();
            if (cytokineScript != null)
            {
                cytokineScript.Deactivate();
                return;
            }

            // Fall back to tutorial cytokine
            TutorialCytokine tutorialCytokine = closestCytokine.GetComponent<TutorialCytokine>();
            if (tutorialCytokine != null)
            {
                tutorialCytokine.Deactivate();
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
                if (bacteriaScript != null) { bacteriaScript.Die(); return; }

                TutorialBacteria tutBacteria = closestBacteria.GetComponent<TutorialBacteria>();
                if (tutBacteria != null) tutBacteria.Die();
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
