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
                // Rotation around the planet
                float rotationAmount = -moveInput * moveSpeed * Time.deltaTime;
                transform.position = Quaternion.Euler(0, 0, rotationAmount) *
                                    (transform.position - currentPlanet.position) +
                                    currentPlanet.position;

                // Make sure the player stays on the surface
                KeepPlayerOnPlanetSurface();

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
        // Landing on a planet
        if (other.CompareTag("Planet") && isJumping)
        {
            Transform planet = other.transform;
            AttachToPlanet(planet);
        }
    }

    public void ResetPlayer(Transform startPlanet)
    {
        currentPlanet = startPlanet;
        isJumping = false;
        firstJumpExecuted = false;
        SetHasJumped(false);

        // Sécurité rigide : forcer la position et la rotation du joueur
        Vector3 directionFromCenter = Vector3.up;
        float planetRadius = currentPlanet.GetComponent<CircleCollider2D>().radius * currentPlanet.localScale.x;
        float offset = GetComponent<SpriteRenderer>().bounds.extents.y * surfaceOffsetMultiplier;
        transform.position = currentPlanet.position + directionFromCenter * (planetRadius + offset);

        float angle = Mathf.Atan2(directionFromCenter.y, directionFromCenter.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        // Reset rigidbody
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        animator.SetBool("isJumping", false);
        animator.SetBool("isMoving", false);

        Debug.Log("✅ Joueur repositionné sur la planète de départ.");
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
        
        // Check for special planet effects
        Planet planetScript = planet.GetComponent<Planet>();
        if (planetScript != null && planetScript.isSpecialPlanet)
        {
            // Apply special effect
            planetScript.ApplySpecialEffect(this);
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

        // Exact position where the circles are tangent
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
}