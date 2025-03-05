using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float orbitSpeed = 30f;
    public float moveSpeed = 80f;
    public float jumpForce = 4f;
    private Transform currentPlanet;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isJumping = false;
    private float moveInput;
    private bool firstJumpExecuted = false;

    // Événement pour le premier saut
    public delegate void PlayerActionHandler();
    public static event PlayerActionHandler OnFirstJump;

    // Variable statique pour le GameManager
    public static bool HasJumped { get; private set; } = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Attacher directement le joueur à la planète de départ
        PlanetSpawner spawner = FindFirstObjectByType<PlanetSpawner>();
        if (spawner != null && spawner.startPlanet != null)
        {
            AttachToPlanet(spawner.startPlanet.transform);
            Debug.Log("✅ Joueur attaché à la planète de départ.");
        }
        else
        {
            Debug.LogError("❌ Pas de planète de départ définie !");
        }
    }

    void Update()
    {
        moveInput = Input.GetAxis("Horizontal");

        if (currentPlanet != null && !isJumping)
        {
            float rotationAmount = -moveInput * moveSpeed * Time.deltaTime;
            transform.position = Quaternion.Euler(0, 0, rotationAmount) * (transform.position - currentPlanet.position) + currentPlanet.position;

            animator.SetBool("isMoving", moveInput != 0);
        }

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
    }

    void Jump()
    {
        if (currentPlanet == null || isJumping) return;

        isJumping = true;
        transform.parent = null;
        animator.SetBool("isJumping", true);

        Vector2 jumpDirection = (transform.position - currentPlanet.position).normalized;
        rb.linearVelocity = jumpDirection * jumpForce;

        // Marquer que le joueur a sauté (statique)
        HasJumped = true;

        // Déclencher l'événement lors du premier saut
        if (!firstJumpExecuted)
        {
            firstJumpExecuted = true;
            Debug.Log("🟢 PREMIER SAUT DÉTECTÉ - OnFirstJump va être appelé");

            if (OnFirstJump != null)
            {
                OnFirstJump();
                Debug.Log("🟢 OnFirstJump a été appelé avec succès");
            }
            else
            {
                Debug.LogError("🔴 ERREUR: Personne n'écoute l'événement OnFirstJump!");
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

    void AttachToPlanet(Transform newPlanet)
    {
        isJumping = false;
        currentPlanet = newPlanet;
        transform.parent = newPlanet;
        rb.linearVelocity = Vector2.zero;

        // ✅ 1️⃣ Trouver la direction du centre de la planète
        Vector3 directionToCenter = (newPlanet.position - transform.position).normalized;

        // ✅ 2️⃣ Obtenir le rayon exact de la planète
        CircleCollider2D planetCollider = newPlanet.GetComponent<CircleCollider2D>();
        if (planetCollider == null)
        {
            Debug.LogError("❌ ERREUR: La planète n'a pas de `CircleCollider2D` !");
            return;
        }
        float planetRadius = planetCollider.radius * newPlanet.transform.localScale.x; // Prend en compte la taille de la planète

        // ✅ 3️⃣ Obtenir la hauteur du joueur (collider)
        Collider2D playerCollider = GetComponent<Collider2D>();
        float playerRadius = (playerCollider != null) ? playerCollider.bounds.extents.y : 0.5f; // Valeur par défaut si pas de collider

        // ✅ 4️⃣ Calculer l’offset pour placer le joueur sur la surface de la planète
        float surfaceOffset = playerRadius + 0.1f; // Ajout de 0.1f pour éviter de pénétrer la planète

        // ✅ 5️⃣ Positionner le joueur SUR la surface de la planète
        Vector3 targetPosition = newPlanet.position + (-directionToCenter * (planetRadius + surfaceOffset));
        transform.position = targetPosition;

        // ✅ 6️⃣ Calculer l’angle pour aligner le joueur avec la planète
        float targetAngle = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0, 0, targetAngle);

        // ✅ 7️⃣ Débogage pour voir si la position est correcte
        Debug.DrawLine(newPlanet.position, targetPosition, Color.green, 2f); // Ligne verte pour voir la position correcte

        // ✅ 8️⃣ Mise à jour de l’animation
        animator.SetBool("isJumping", false);
    }


    // Méthode utilitaire pour forcer un saut (usage externe)
    public static void SetHasJumped(bool value)
    {
        HasJumped = value;
    }
}