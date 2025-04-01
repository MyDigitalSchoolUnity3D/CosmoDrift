using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Paramètres de mouvement
    public float moveSpeed = 80f;
    public float jumpForce = 4f;

    // Ajouter cette variable dans la liste des variables au début de la classe
    public float surfaceOffsetMultiplier = 1.0f; // Multiplicateur pour ajuster finement l'offset

    // Références
    private Transform currentPlanet;
    private Rigidbody2D rb;
    private Animator animator;

    // États
    private bool isJumping = false;
    private bool firstJumpExecuted = false;

    // Événement pour le premier saut
    public delegate void PlayerActionHandler();
    public static event PlayerActionHandler OnFirstJump;

    // Variable statique pour le GameManager
    public static bool HasJumped { get; private set; } = false;

    void Start()
    {
        // Obtenir les composants nécessaires
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Configurer le Rigidbody
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        // Attacher à la planète de départ
        PlanetSpawner spawner = FindFirstObjectByType<PlanetSpawner>();
        if (spawner != null && spawner.startPlanet != null)
        {
            // Vérifier que currentPlanet est bien défini
            currentPlanet = spawner.startPlanet.transform;
            AttachToPlanet(currentPlanet);
            Debug.Log($"✅ Joueur attaché à la planète de départ: {currentPlanet.name}");
        }
    }

    void Update()
    {
        // Vérifier que currentPlanet est bien défini
        if (currentPlanet == null)
        {
            Debug.LogWarning("⚠️ currentPlanet est null dans Update!");
            return;
        }

        // Mouvement du joueur sur la planète
        if (!isJumping)
        {
            float moveInput = Input.GetAxis("Horizontal");

            // Si les axes ne fonctionnent pas, utiliser les touches directement
            if (moveInput == 0)
            {
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                    moveInput = -1f;
                else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                    moveInput = 1f;
            }

            if (moveInput != 0)
            {
                // Rotation autour de la planète
                float rotationAmount = -moveInput * moveSpeed * Time.deltaTime;
                transform.position = Quaternion.Euler(0, 0, rotationAmount) *
                                    (transform.position - currentPlanet.position) +
                                    currentPlanet.position;

                // S'assurer que le joueur reste sur la surface
                KeepPlayerOnPlanetSurface();

                // Animation
                animator.SetBool("isMoving", true);
            }
            else
            {
                animator.SetBool("isMoving", false);
            }
        }

        // Sauter
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    public void Jump()
    {
        // Vérifier explicitement que currentPlanet n'est pas null
        if (currentPlanet == null)
        {
            Debug.LogError("❌ Impossible de sauter: currentPlanet est null!");
            return;
        }

        if (isJumping)
        {
            Debug.Log("⚠️ Déjà en train de sauter");
            return;
        }

        Debug.Log($"🟢 Saut depuis la planète {currentPlanet.name}");

        // Activer l'état de saut
        isJumping = true;
        animator.SetBool("isJumping", true);

        // Direction du saut (du centre de la planète vers le joueur)
        Vector2 jumpDirection = (transform.position - currentPlanet.position).normalized;

        // Appliquer la force de saut
        rb.linearVelocity = jumpDirection * jumpForce;

        // Marquer le premier saut
        HasJumped = true;

        // Déclencher l'événement du premier saut
        if (!firstJumpExecuted)
        {
            firstJumpExecuted = true;
            if (OnFirstJump != null)
            {
                OnFirstJump();
                Debug.Log("🟢 OnFirstJump a été appelé");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Atterrissage sur une planète
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
        if (planet == null)
        {
            Debug.LogError("❌ Tentative d'attacher à une planète null!");
            return;
        }

        // Reset de l'état
        isJumping = false;
        currentPlanet = planet;
        rb.linearVelocity = Vector2.zero;

        // Positionner le joueur sur la surface de la planète
        KeepPlayerOnPlanetSurface();

        // Animation
        animator.SetBool("isJumping", false);

        Debug.Log($"✅ Joueur attaché à {planet.name}");
    }

    void KeepPlayerOnPlanetSurface()
    {
        if (currentPlanet == null) return;

        // Direction du centre de la planète vers le joueur
        Vector3 dirFromPlanet = (transform.position - currentPlanet.position).normalized;

        // Utiliser un Raycast depuis le centre de la planète pour détecter la surface
        RaycastHit2D hit = Physics2D.Raycast(
            currentPlanet.position,               // Point de départ au centre de la planète
            dirFromPlanet,                        // Direction vers le joueur
            20f,                                  // Distance maximale
            LayerMask.GetMask("Planet")           // Masque de couche (assurez-vous que vos planètes sont sur la couche "Planet")
        );

        // Si on touche la surface de la planète
        if (hit.collider != null && hit.collider.transform == currentPlanet)
        {
            // Obtenir le point exact de la surface de la planète
            Vector3 surfacePoint = hit.point;

            // Calculer un décalage pour le joueur (basé sur la taille du sprite)
            float playerOffset = 0.1f; // Offset minimal

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Utiliser la hauteur du sprite pour un positionnement plus précis
                playerOffset = spriteRenderer.bounds.extents.y * surfaceOffsetMultiplier;
            }

            // Position finale du joueur = point de surface + un peu d'offset dans la direction
            transform.position = surfacePoint + (dirFromPlanet * playerOffset);

            // Debug visuel
            Debug.DrawLine(currentPlanet.position, surfacePoint, Color.yellow, 0.1f);
            Debug.DrawLine(surfacePoint, transform.position, Color.blue, 0.1f);
        }
        else
        {
            // Fallback au cas où le raycast échoue
            // Garder l'ancienne méthode comme secours
            float planetRadius = 0f;
            CircleCollider2D planetCollider = currentPlanet.GetComponent<CircleCollider2D>();
            if (planetCollider != null)
            {
                planetRadius = planetCollider.radius * currentPlanet.transform.localScale.x;
            }

            float pivotOffset = 0.3f;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                pivotOffset = spriteRenderer.bounds.extents.y * surfaceOffsetMultiplier;
                pivotOffset = Mathf.Max(pivotOffset, 0.3f);
            }

            transform.position = currentPlanet.position + dirFromPlanet * (planetRadius + pivotOffset);

            Debug.LogWarning("⚠️ Raycast n'a pas détecté la surface de la planète. Méthode alternative utilisée.");
        }

        // Orientation perpendiculaire à la surface
        float angle = Mathf.Atan2(dirFromPlanet.y, dirFromPlanet.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    public static void SetHasJumped(bool value)
    {
        HasJumped = value;
    }
}