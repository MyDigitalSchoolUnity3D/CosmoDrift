using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement parameters
    public float moveSpeed = 80f;
    public float jumpForce = 4f;
    public float surfaceOffsetMultiplier = 1.0f;

    // References
    private Transform currentPlanet;
    private Rigidbody2D rb;
    private Animator animator;

    // States
    private bool isJumping = false;
    private bool firstJumpExecuted = false;

    // Event for the first jump
    public delegate void PlayerActionHandler();
    public static event PlayerActionHandler OnFirstJump;

    // Static variable for GameManager
    public static bool HasJumped { get; private set; } = false;

    [Header("Audio")]
    public AudioClip landingSound;
    public AudioClip jumpSound;
    private AudioSource audioSource;
    [Range(0f, 1f)]
    public float soundVolume = 1f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        // Attach to the starting planet
        PlanetSpawner spawner = FindFirstObjectByType<PlanetSpawner>();
        if (spawner != null && spawner.startPlanet != null)
        {
            currentPlanet = spawner.startPlanet.transform;
            InitialPositioningOnStartPlanet();
        }

        SetupAudio();
    }
    
    private void InitialPositioningOnStartPlanet()
    {
        if (currentPlanet == null) return;
        
        transform.SetParent(currentPlanet);
        
        CircleCollider2D planetCollider = currentPlanet.GetComponent<CircleCollider2D>();
        CircleCollider2D playerCollider = GetComponent<CircleCollider2D>();
        
        if (planetCollider != null && playerCollider != null)
        {
            float planetRadius = planetCollider.radius * Mathf.Max(currentPlanet.lossyScale.x, currentPlanet.lossyScale.y);
            float playerRadius = playerCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
            
            Vector2 upDirection = Vector2.up;
            Vector2 planetPos = currentPlanet.position;
            
            Vector2 playerPos = planetPos + upDirection * (planetRadius + playerRadius);
            transform.position = playerPos;
            
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        
        KeepPlayerOnPlanetSurface();
    }

    void Update()
    {
        if (currentPlanet == null) return;

        // Player movement on the planet
        if (!isJumping)
        {
            float moveInput = Input.GetAxis("Horizontal");

            if (moveInput == 0)
            {
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                    moveInput = -1f;
                else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                    moveInput = 1f;
            }

            if (moveInput != 0)
            {
                float rotationAmount = -moveInput * moveSpeed * Time.deltaTime;
                
                Vector2 dirToPlayer = (transform.position - currentPlanet.position);
                Vector2 rotatedDir = Quaternion.Euler(0, 0, rotationAmount) * dirToPlayer;
                
                Vector2 newDirNormalized = rotatedDir.normalized;
                
                CircleCollider2D planetCollider = currentPlanet.GetComponent<CircleCollider2D>();
                CircleCollider2D playerCollider = GetComponent<CircleCollider2D>();
                
                if (planetCollider != null && playerCollider != null)
                {
                    float planetRadius = planetCollider.radius * Mathf.Max(currentPlanet.lossyScale.x, currentPlanet.lossyScale.y);
                    float playerRadius = playerCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
                    
                    Vector2 newPosition = (Vector2)currentPlanet.position + newDirNormalized * (planetRadius + playerRadius);
                    transform.position = newPosition;
                    
                    float angle = Mathf.Atan2(newDirNormalized.y, newDirNormalized.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
                }
                
                animator.SetBool("isMoving", true);
            }
            else
            {
                animator.SetBool("isMoving", false);
            }
        }

        // Jump
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    public void Jump()
    {
        if (currentPlanet == null || isJumping) return;

        transform.SetParent(null);

        isJumping = true;
        animator.SetBool("isJumping", true);
        
        PlaySound(jumpSound);

        Vector2 jumpDirection = (transform.position - currentPlanet.position).normalized;
        rb.linearVelocity = jumpDirection * jumpForce;

        HasJumped = true;

        if (!firstJumpExecuted)
        {
            firstJumpExecuted = true;
            if (OnFirstJump != null)
            {
                OnFirstJump();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Planet") && isJumping)
        {
            AttachToPlanet(other.transform);
        }
    }

    void AttachToPlanet(Transform planet)
    {
        if (planet == null) return;

        isJumping = false;
        currentPlanet = planet;
        rb.linearVelocity = Vector2.zero;

        PlaySound(landingSound);

        transform.SetParent(planet);
        KeepPlayerOnPlanetSurface();
        animator.SetBool("isJumping", false);
    }

    void KeepPlayerOnPlanetSurface()
    {
        if (currentPlanet == null) return;

        CircleCollider2D planetCollider = currentPlanet.GetComponent<CircleCollider2D>();
        CircleCollider2D playerCollider = GetComponent<CircleCollider2D>();
        
        if (planetCollider == null || playerCollider == null) return;

        float planetRadius = planetCollider.radius * Mathf.Max(currentPlanet.lossyScale.x, currentPlanet.lossyScale.y);
        float playerRadius = playerCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);

        Vector2 dirToPlayer = (transform.position - currentPlanet.position);
        
        float distance = dirToPlayer.magnitude;
        if (distance < 0.1f)
        {
            dirToPlayer = Vector2.up;
        }
        else
        {
            dirToPlayer /= distance;
        }

        Vector2 tangentPosition = (Vector2)currentPlanet.position + dirToPlayer * (planetRadius + playerRadius);
        transform.position = tangentPosition;

        float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    public static void SetHasJumped(bool value)
    {
        HasJumped = value;
    }

    public void ResetPlayer(Transform startPlanet)
    {
        if (startPlanet == null) return;

        currentPlanet = startPlanet;
        isJumping = false;
        firstJumpExecuted = false;
        SetHasJumped(false);

        transform.SetParent(currentPlanet);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        CircleCollider2D planetCollider = currentPlanet.GetComponent<CircleCollider2D>();
        CircleCollider2D playerCollider = GetComponent<CircleCollider2D>();
        
        if (planetCollider != null && playerCollider != null)
        {
            float planetRadius = planetCollider.radius * Mathf.Max(currentPlanet.lossyScale.x, currentPlanet.lossyScale.y);
            float playerRadius = playerCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
            
            Vector2 upDirection = Vector2.up;
            Vector2 planetPosition = currentPlanet.position;
            
            Vector2 playerPosition = planetPosition + upDirection * (planetRadius + playerRadius);
            transform.position = playerPosition;
            
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.position = currentPlanet.position + Vector3.up * 2f;
            transform.rotation = Quaternion.identity;
        }

        if (animator != null)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isMoving", false);
        }
    }

    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = soundVolume;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        
        audioSource.volume = soundVolume;
        audioSource.PlayOneShot(clip);
    }
}