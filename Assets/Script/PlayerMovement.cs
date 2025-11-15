using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float speedIncreasePerDistance = 0.1f; // Speed increase per 100 units

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravity = 1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Mobile Controls")]
    [SerializeField] private bool useTouchControls = true;

    [Header("Player Animator")]
    [SerializeField] private Animator animator;

    [Header("Game Manager")]
    [SerializeField] private GameManager gameManager;

    [Header("Game Manager")]
    [SerializeField] private Transform DefaultPlayerPos;

    [Header("Power-ups")]
    [SerializeField] private GameObject shieldObject;
    [SerializeField] private float shieldDuration = 15f;
    [SerializeField] private float speedBoostMultiplier = 1.25f;
    [SerializeField] private float speedBoostDuration = 20f;


    private static readonly int StateParam = Animator.StringToHash("State");

    private const int STATE_RUN = 1;
    private const int STATE_JUMP = 2;

    private int currentAnimState;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float distanceTraveled;
    private bool isDead;
    private bool collidedbyObstacles;
    private bool collidedbyWater;
    private bool hasShield;
    private bool hasSpeedBoost;
    private float speedBeforeBoost;
    private float shieldTimer;
    private float speedBoostTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = startSpeed;
        rb.gravityScale = gravity;

        jumpForce = Mathf.Sqrt(2 * gravity * Mathf.Abs(Physics2D.gravity.y) * jumpHeight);

        // Ensure groundCheck exists
        if (groundCheck == null)
        {
            GameObject check = new GameObject("GroundCheck");
            check.transform.parent = transform;
            check.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = check.transform;
        }

        if (shieldObject != null)
            shieldObject.SetActive(false);

        SetAnimationState(STATE_RUN);
    }

    private void Update()
    {
        if (isDead) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        //Debug.Log("Is Grounded: " + isGrounded);

        HandleJumpInput();
        IncreaseSpeed();
        UpdateAnimation();
        UpdatePowerUps();

        distanceTraveled += currentSpeed * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    private void HandleJumpInput()
    {
        bool jumpPressed = false;

        if (useTouchControls)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    jumpPressed = true;
                }
            }
        }

        // keyboard for testing
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            jumpPressed = true;
        }

        if (jumpPressed && isGrounded)
        {
            Jump();
        }
    }

    private void Jump()
    {
        SetAnimationState(STATE_JUMP);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void IncreaseSpeed()
    {
        // Increase speed based on distance (every 100 units)
        float distanceSpeedIncrease = (distanceTraveled / 100f) * speedIncreasePerDistance * Time.deltaTime;

        currentSpeed += distanceSpeedIncrease;

        // Clamp to max speed
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
    }

    public void Die()
    {
        if (isDead) return;

        if (collidedbyObstacles) 
        {
            if (hasShield)
            {
                transform.position = new Vector3(transform.position.x + 3f, transform.position.y, transform.position.z);
                DeactivateShield(); //Shield protect player
                
                collidedbyObstacles = false;
                return;
            }
        }

        isDead = true;
        rb.linearVelocity = Vector2.zero;
        animator.speed = 0f;
        gameManager.GameOver();
    }

    public void ResetPlayer()
    {
        isDead = false;
        currentSpeed = startSpeed;
        distanceTraveled = 0;
        transform.position = DefaultPlayerPos.position;
        rb.linearVelocity = Vector2.zero;
        animator.speed = 1.25f;

        DeactivateShield();
        DeactivateSpeedBoost();

    }

    private void SetAnimationState(int newState)
    {
        if (animator == null || currentAnimState == newState) return;

        currentAnimState = newState;
        animator.SetInteger(StateParam, newState);
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        if (isGrounded)
        {
            if(currentAnimState != STATE_RUN)
            {
                SetAnimationState(STATE_RUN);
            }
        }
        else
        {
            SetAnimationState(STATE_JUMP);
        }
    }

    private void UpdatePowerUps()
    {
        // Update Shield Timer
        if (hasShield)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                DeactivateShield();
            }
        }

        // Update Speed Boost Timer
        if (hasSpeedBoost)
        {
            speedBoostTimer -= Time.deltaTime;
            if (speedBoostTimer <= 0f)
            {
                DeactivateSpeedBoost();
            }
        }
    }

    public void ActivateShield()
    {
        hasShield = true;
        shieldTimer = shieldDuration;

        if (shieldObject != null)
        {
            shieldObject.SetActive(true);
        }
    }

    private void DeactivateShield()
    {
        hasShield = false;
        shieldTimer = 0f;

        if (shieldObject != null)
            shieldObject.SetActive(false);
    }

    public void ActivateSpeedBoost()
    {
        if (!hasSpeedBoost)
        {
            hasSpeedBoost = true;
            speedBoostTimer = speedBoostDuration;
            speedBeforeBoost = currentSpeed;
            currentSpeed *= speedBoostMultiplier;
        }
    }

    private void DeactivateSpeedBoost()
    {
        if (hasSpeedBoost)
        {
            hasSpeedBoost = false;
            speedBoostTimer = 0f;

            // Restore speed proportionally (in case speed increased naturally during boost)
            float speedRatio = currentSpeed / (speedBeforeBoost * speedBoostMultiplier);
            currentSpeed = speedBeforeBoost * speedRatio;
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacles"))
        {
            collidedbyObstacles = true;
            Die();
        }

        if (collision.gameObject.CompareTag("Water"))
        {
            Die();
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shield"))
        {
            ActivateShield();
            other.gameObject.SetActive(false);

        }

        if (other.gameObject.CompareTag("SpeedBoost"))
        {
            ActivateSpeedBoost();
            other.gameObject.SetActive(false);
        }

        if (other.gameObject.CompareTag("Coin"))
        {
            gameManager.AddCoin();
            other.gameObject.SetActive(false);
        }

    }
}