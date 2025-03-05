using UnityEngine;

public class Planet : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public float fallSpeed = 0.5f;
    public float planetSize = 1f;
    private bool rotatingRight;
    private bool gameStarted = false;
    private bool isStartPlanet = false;

    void Start()
    {
        // S'assurer que le trigger est activé pour la collision
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        // Vérifier si c'est la planète de départ de plusieurs façons

        // Méthode 1: vérifier avec le tag
        if (gameObject.CompareTag("StartPlanet"))
        {
            isStartPlanet = true;
        }
        // Méthode 2: vérifier avec la référence dans le PlanetSpawner
        else
        {
            PlanetSpawner spawner = Object.FindFirstObjectByType<PlanetSpawner>();
            if (spawner != null && gameObject == spawner.startPlanet)
            {
                isStartPlanet = true;
                gameObject.tag = "StartPlanet"; // S'assurer que le tag est correct
            }
        }

        // Si c'est la planète de départ, ne pas la faire tourner
        if (isStartPlanet)
        {
            rotationSpeed = 0f;
            Debug.Log("Planète de départ identifiée: rotation désactivée.");
            // Ne PAS toucher à la taille de la planète de départ
        }
        else
        {
            // Appliquer la taille seulement pour les planètes qui ne sont PAS la planète de départ
            ApplyPlanetSize();
        }
    }

    public void Initialize(float speed, float size, float fall)
    {
        rotationSpeed = speed;
        fallSpeed = fall;

        // Ne pas modifier la taille de la planète de départ
        if (!isStartPlanet)
        {
            planetSize = size;
            ApplyPlanetSize();
        }

        rotatingRight = Random.value > 0.5f;

        // Si c'est la planète de départ, ne pas la faire tourner
        if (isStartPlanet)
        {
            rotationSpeed = 0f;
        }
    }

    // Support pour l'ancienne méthode Initialize pour la compatibilité
    public void Initialize(float speed, float size)
    {
        Initialize(speed, size, fallSpeed);
    }

    public void ApplyPlanetSize()
    {
        // N'appliquer la taille qu'aux planètes qui ne sont PAS la planète de départ
        if (!isStartPlanet)
        {
            // Appliquer l'échelle visuelle
            transform.localScale = new Vector3(planetSize, planetSize, planetSize);

            // S'assurer que le collider correspond à la taille visuelle
            CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
            if (circleCollider != null)
            {
                // Le rayon reste le même car l'échelle est appliquée automatiquement au collider
                Debug.Log($"Planète {name}: Taille visuelle = {planetSize}, Rayon du collider = {circleCollider.radius}");
            }
        }
    }

    void Update()
    {
        // Les planètes tournent tout le temps (sauf la planète de départ qui a rotationSpeed = 0)
        float direction = rotatingRight ? 1 : -1;
        transform.Rotate(Vector3.forward * rotationSpeed * direction * Time.deltaTime);

        // Mais elles ne descendent que quand le jeu a commencé
        if (gameStarted)
        {
            Vector3 oldPosition = transform.position;
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            // Uniquement pour le débogage - supprimer ensuite pour éviter de remplir la console
            //if (Time.frameCount % 60 == 0) // Affiche le message seulement toutes les 60 frames
            //{
            //    Debug.Log($"🟢 Planète {name} tombe: position Y {transform.position.y:F2}, déplacement {(oldPosition.y - transform.position.y):F4}");
            //}

            // Détruire la planète quand elle n'est plus visible
            if (!IsVisibleFrom(Camera.main))
            {
                Debug.Log($"🟢 Planète {name} sort de l'écran - destruction");
                Destroy(gameObject);
            }
        }
    }

    public void StartFalling()
    {
        Debug.Log($"🟢 StartFalling appelé sur {name}");

        // Activer la chute pour cette planète
        gameStarted = true;

        // S'assurer que la vitesse de chute est positive - utiliser une valeur par défaut si nécessaire
        if (fallSpeed <= 0)
        {
            PlanetSpawner spawner = FindFirstObjectByType<PlanetSpawner>();
            if (spawner != null)
            {
                fallSpeed = Random.Range(spawner.minFallSpeed, spawner.maxFallSpeed);
            }
            else
            {
                fallSpeed = 0.5f; // Valeur par défaut si PlanetSpawner n'est pas trouvé
            }
            Debug.Log($"🟢 Correction de la vitesse de chute à {fallSpeed} car elle était à 0");
        }

        // Ajouter une force vers le bas via Rigidbody2D si disponible
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.gravityScale = fallSpeed * 2; // Utiliser fallSpeed pour influencer la gravité
        }

        Debug.Log($"🟢 La planète {name} va maintenant tomber avec vitesse {fallSpeed}");
    }

    bool IsVisibleFrom(Camera cam)
    {
        if (cam == null) return false;

        Vector3 screenPoint = cam.WorldToViewportPoint(transform.position);
        float buffer = 0.2f;

        return screenPoint.x > -buffer &&
               screenPoint.x < (1 + buffer) &&
               screenPoint.y > -buffer;
    }
}
