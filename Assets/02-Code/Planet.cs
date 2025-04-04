using UnityEngine;

public class Planet : MonoBehaviour
{
    // Configuration
    public float rotationSpeed = 10f;
    public float fallSpeed = 0.5f;
    public float planetSize = 1f;
    public bool preventDestruction = false;  // Empêcher la destruction de la planète
    
    // État
    private bool rotatingRight;
    private bool gameStarted = false;
    private bool isStartPlanet = false;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private Camera mainCamera;
    
    // Cache
    private float screenBottom;
    private float spriteRadius;
    
    void Awake()
    {
        // Cache les composants
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        mainCamera = Camera.main;
        
        // Configuration collider  
        if (circleCollider != null)
        {
            circleCollider.isTrigger = true;
        }
    }
    
    void Start()
    {
        // Identifier la planète de départ
        isStartPlanet = gameObject.CompareTag("StartPlanet");
        
        // Si c'est la planète de départ, pas de rotation et empêcher la destruction
        if (isStartPlanet)
        {
            rotationSpeed = 0f;
            preventDestruction = true;  // La planète de départ ne doit pas être détruite
        }
    }

    public void Initialize(float speed, float size, float fall, Sprite texture = null)
    {
        // Skip pour la planète de départ
        if (isStartPlanet) return;
        
        rotationSpeed = speed;
        fallSpeed = fall;
        planetSize = size;
        rotatingRight = Random.Range(0, 2) == 0;
        
        // Appliquer la texture
        if (texture != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = texture;
            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = 1;
        }
        
        // Appliquer la taille
        ApplyPlanetSize();
    }

    public void ApplyPlanetSize()
    {
        // Skip pour la planète de départ
        if (isStartPlanet) return;
        
        // Appliquer l'échelle
        transform.localScale = new Vector3(planetSize, planetSize, planetSize);
        
        // Ajuster le collider basé sur le sprite
        if (circleCollider != null && spriteRenderer != null && spriteRenderer.sprite != null)
        {
            circleCollider.offset = Vector2.zero;
            
            // Obtenir le rayon du sprite
            spriteRadius = spriteRenderer.sprite.bounds.size.x / 2.5f;
            circleCollider.radius = spriteRadius;
        }
    }

    // Méthode pour forcer la chute de la planète avec une vitesse minimale
    public void ForceStartFalling()
    {
        gameStarted = true;
        
        // Assurer une vitesse de chute minimale
        if (fallSpeed <= 0.1f)
            fallSpeed = 0.5f;
            
        // Si le joueur est encore attaché, le détacher
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null && player.parent == transform)
            player.SetParent(null);
    }
    
    public void StartFalling()
    {
        gameStarted = true;
        
        // Assurer une vitesse de chute minimale aussi dans cette méthode
        if (fallSpeed <= 0.1f)
            fallSpeed = 0.3f;
    }
    
    void Update()
    {
        // Rotation (sauf planète de départ)
        if (!isStartPlanet)
        {
            int direction = rotatingRight ? 1 : -1;
            transform.Rotate(Vector3.forward * rotationSpeed * direction * Time.deltaTime);
        }
        
        // Chute si le jeu a commencé
        if (gameStarted)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
            
            // Destruction si hors écran (sauf pour la planète de départ)
            if (IsOutOfScreen())
            {
                if (!preventDestruction)
                {
                    Destroy(gameObject);
                }
                else if (isStartPlanet)
                {
                    // Pour la planète de départ, ne pas la détruire mais la cacher
                    // Elle sera repositionnée correctement lors du reset
                    gameObject.SetActive(false);
                }
            }
        }
    }
    
    // Méthode pour réinitialiser la position si nécessaire
    private void ResetPosition()
    {
        if (!mainCamera) mainCamera = Camera.main;
        if (!mainCamera) return;
        
        // Calculer la position au-dessus de l'écran
        float camHeight = 2f * mainCamera.orthographicSize;
        float screenTop = mainCamera.transform.position.y + camHeight/2;
        Vector3 newPosition = new Vector3(transform.position.x, screenTop + 5f, transform.position.z);
        transform.position = newPosition;
    }
    
    // Méthode pour réinitialiser la planète
    public void ResetPlanet()
    {
        gameStarted = false;
        rotatingRight = Random.Range(0, 2) == 0;
        
        // Réactiver la planète si elle a été désactivée
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    bool IsOutOfScreen()
    {
        if (!mainCamera) mainCamera = Camera.main;
        if (!mainCamera) return false;
        
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        // Augmentons la marge pour s'assurer que les planètes sont bien détruites
        return viewportPosition.y < -0.3f;
    }
}
