using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement parameters
    public float moveSpeed = 80f;
    public float jumpForce = 4f;
    public float surfaceOffsetMultiplier = 1.0f; // Multiplier for fine adjustment of the offset

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
    public AudioClip jumpSound;  // New sound for jumping
    private AudioSource audioSource;
    
    void Start()
    {
        // Get necessary components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Configure the Rigidbody
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        // Attach to the starting planet
        PlanetSpawner spawner = FindFirstObjectByType<PlanetSpawner>();
        if (spawner != null && spawner.startPlanet != null)
        {
            // Make sure currentPlanet is properly defined
            currentPlanet = spawner.startPlanet.transform;
            
            // Positionner correctement sur la planète de départ
            InitialPositioningOnStartPlanet();
        }

        // Audio configuration
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    // Méthode pour positionner correctement le joueur sur la planète initiale
    private void InitialPositioningOnStartPlanet()
    {
        if (currentPlanet == null) return;
        
        // S'assurer que le joueur est bien un enfant de la planète
        transform.SetParent(currentPlanet);
        
        // Positionner au-dessus de la planète (à 12h)
        CircleCollider2D planetCollider = currentPlanet.GetComponent<CircleCollider2D>();
        CircleCollider2D playerCollider = GetComponent<CircleCollider2D>();
        
        if (planetCollider != null && playerCollider != null)
        {
            // Calculer les rayons réels
            float planetRadius = planetCollider.radius * currentPlanet.localScale.x;
            float playerRadius = playerCollider.radius * transform.localScale.x;
            
            // Positionner le joueur au-dessus de la planète (direction y+)
            Vector2 upDirection = Vector2.up;
            Vector2 planetPos = currentPlanet.position;
            
            // Position où les cercles sont tangents
            Vector2 playerPos = planetPos + upDirection * (planetRadius + playerRadius);
            transform.position = playerPos;
            
            // Orienter le joueur perpendiculairement à la surface
            transform.rotation = Quaternion.Euler(0, 0, 0); // Droit vers le haut
        }
        
        // Appliquer la méthode générale pour être sûr
        KeepPlayerOnPlanetSurface();
    }

    void Update()
    {
        // Check that currentPlanet is properly defined
        if (currentPlanet == null)
        {
            return;
        }

        // Player movement on the planet
        if (!isJumping)
        {
            float moveInput = Input.GetAxis("Horizontal");

            // If the axes don't work, use the keys directly
            if (moveInput == 0)
            {
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                    moveInput = -1f;
                else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                    moveInput = 1f;
            }

            if (moveInput != 0)
            {
                // Calculate rotation angle
                float rotationAmount = -moveInput * moveSpeed * Time.deltaTime;
                
                // Rotate the player around the planet
                Vector2 dirToPlayer = (transform.position - currentPlanet.position);
                Vector2 rotatedDir = Quaternion.Euler(0, 0, rotationAmount) * dirToPlayer;
                
                // Get the normalized direction for orientation
                Vector2 newDirNormalized = rotatedDir.normalized;
                
                // Calculate the new position based on the planet and player colliders
                CircleCollider2D planetCollider = currentPlanet.GetComponent<CircleCollider2D>();
                CircleCollider2D playerCollider = GetComponent<CircleCollider2D>();
                
                if (planetCollider != null && playerCollider != null)
                {
                    float planetRadius = planetCollider.radius * currentPlanet.transform.localScale.x;
                    float playerRadius = playerCollider.radius * transform.localScale.x;
                    
                    // Set the position exactly at the correct distance
                    Vector2 newPosition = (Vector2)currentPlanet.position + newDirNormalized * (planetRadius + playerRadius);
                    transform.position = newPosition;
                    
                    // Orient the player perpendicular to the planet surface
                    float angle = Mathf.Atan2(newDirNormalized.y, newDirNormalized.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
                }
                
                // Animation
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
        // Explicitly check that currentPlanet is not null
        if (currentPlanet == null)
        {
            return;
        }

        if (isJumping)
        {
            return;
        }

        // Detach player from the planet
        transform.SetParent(null);

        // Activate jump state
        isJumping = true;
        animator.SetBool("isJumping", true);
        
        // Play the jump sound
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }

        // Jump direction (from center of planet to player)
        Vector2 jumpDirection = (transform.position - currentPlanet.position).normalized;

        // Apply jump force
        rb.linearVelocity = jumpDirection * jumpForce;

        // Mark the first jump
        HasJumped = true;

        // Trigger the first jump event
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
        Debug.Log($"⚡ Trigger avec {other.tag}");
        // Landing on a planet
        if (other.CompareTag("Planet") && isJumping)
        {
            Transform planet = other.transform;
            AttachToPlanet(planet);
        }
    }

    void AttachToPlanet(Transform planet)
    {
        if (planet == null) return;

        // Reset the state
        isJumping = false;
        currentPlanet = planet;
        rb.linearVelocity = Vector2.zero;

        // Play landing sound
        if (landingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(landingSound);
        }

        // Establish parent-child relationship
        transform.SetParent(planet);

        // Position the player on the surface of the planet
        KeepPlayerOnPlanetSurface();

        // Animation
        animator.SetBool("isJumping", false);
    }

    void KeepPlayerOnPlanetSurface()
    {
        if (currentPlanet == null) return;

        CircleCollider2D planetCollider = currentPlanet.GetComponent<CircleCollider2D>();
        CircleCollider2D playerCollider = GetComponent<CircleCollider2D>();
        
        if (planetCollider == null || playerCollider == null) return;

        // Calculate real radii taking into account scales
        float planetRadius = planetCollider.radius * currentPlanet.transform.localScale.x;
        float playerRadius = playerCollider.radius * transform.localScale.x;

        // Direction from the center of the planet to the player
        Vector2 dirToPlayer = (transform.position - currentPlanet.position).normalized;
        
        // Si la direction est trop petite (joueur au centre), forcer une direction vers le haut
        if (dirToPlayer.magnitude < 0.1f)
        {
            dirToPlayer = Vector2.up;
        }

        // IMPORTANT: Position directly based on colliders, not sprite pivot
        // Keep a fixed distance from planet surface to player collider
        Vector2 tangentPosition = (Vector2)currentPlanet.position + dirToPlayer * (planetRadius + playerRadius);
        
        // Apply the position
        transform.position = tangentPosition;

        // Orientation perpendicular to the surface
        float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    public static void SetHasJumped(bool value)
    {
        HasJumped = value;
    }

    public void ResetPlayer(Transform startPlanet)
    {
        // Safety check for null reference
        if (startPlanet == null)
        {
            Debug.LogError("❌ ResetPlayer: startPlanet is null!");
            return;
        }

        currentPlanet = startPlanet;
        isJumping = false;
        firstJumpExecuted = false;
        SetHasJumped(false);

        // Make the player a child of the planet
        transform.SetParent(currentPlanet);

        // Reset rigidbody first to prevent physics interactions during repositioning
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        // FIXED POSITIONING: Force specific initial position
        CircleCollider2D planetCollider = currentPlanet.GetComponent<CircleCollider2D>();
        CircleCollider2D playerCollider = GetComponent<CircleCollider2D>();
        
        if (planetCollider != null && playerCollider != null)
        {
            // Calculate real radii
            float planetRadius = planetCollider.radius * currentPlanet.localScale.x;
            float playerRadius = playerCollider.radius * transform.localScale.x;
            
            // Position player at the top of the planet (12 o'clock position)
            Vector2 upDirection = Vector2.up;
            Vector2 planetPosition = currentPlanet.position;
            
            // Position based on colliders, not sprite pivot
            Vector2 playerPosition = planetPosition + upDirection * (planetRadius + playerRadius);
            transform.position = playerPosition;
            
            // Force upright orientation (facing upward)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            // Emergency fallback position if colliders aren't found
            transform.position = currentPlanet.position + Vector3.up * 2f;
            transform.rotation = Quaternion.identity;
        }

        // Reset animation
        if (animator != null)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isMoving", false);
        }
        
        Debug.Log("✅ Player repositioned on start planet");
    }
}