using UnityEngine;

public class Planet : MonoBehaviour
{
    // Configuration
    public float rotationSpeed = 10f;
    public float fallSpeed = 0.5f;
    public float planetSize = 1f;
    
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
        
        // Si c'est la planète de départ, pas de rotation
        if (isStartPlanet)
        {
            rotationSpeed = 0f;
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
            
            // Destruction si hors écran
            if (IsOutOfScreen())
            {
                Destroy(gameObject);
            }
        }
    }

    bool IsOutOfScreen()
    {
        if (!mainCamera) mainCamera = Camera.main;
        if (!mainCamera) return false;
        
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        // Vérifier si la planète est complètement sortie par le bas (-0.2 pour laisser une marge)
        return viewportPosition.y < -0.2f;
    }

    public void StartFalling()
    {
        gameStarted = true;
    }
}
