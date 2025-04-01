using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlanetSpawner : MonoBehaviour
{
    public GameObject planetPrefab;
    public GameObject startPlanet;
    public int maxPlanets = 3;
    public float minSpawnDistance = 8f; // Increased for more space between planets
    public float minVerticalDistance = 5f; // Minimum vertical distance between planets
    public float minPlanetSize = 0.8f;
    public float maxPlanetSize = 1.5f;

    // Fall speed configuration
    [Header("Fall Speed Configuration")]
    [Range(0.1f, 2.0f)] public float minFallSpeed = 0.2f;
    [Range(0.1f, 2.0f)] public float maxFallSpeed = 1.2f;
    public float spawnHeightAboveCamera = 3f; // Spawn height above camera

    // Textures for planets
    [Header("Planet Textures")]
    public List<Sprite> planetTextures = new List<Sprite>();

    public int initialPlanets = 3; // Number of planets at start
    public float initialSpawnHeight = 5f; // Spawn height for initial planets

    [Header("Cascade Settings")]
    [Range(0f, 1f)] public float cascadeIntensity = 0.7f; // Controls the strength of the cascade effect
    [Range(-1f, 1f)] public float cascadeDirection = 0.0f; // Negative = left, Positive = right, 0 = zigzag
    [Range(0f, 10f)] public float verticalSpacing = 3f; // Vertical spacing between planets
    private int lastSpawnDirection = 1; // For zigzag effect

    private List<GameObject> planets = new List<GameObject>();
    private bool gameStarted = false;
    private float lastSpawnY = 0f;

    void Start()
    {
        // Make sure the starting planet is properly configured
        if (startPlanet != null)
        {
            // Add a special tag for the starting planet
            startPlanet.tag = "StartPlanet";
            planets.Add(startPlanet);

            // DO NOT modify the size of the starting planet - it's defined in Unity
        }
        else
        {
            return;
        }

        // Spawn initial planets
        SpawnInitialPlanets();
    }

    void SpawnInitialPlanets()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        float middleX = cam.transform.position.x;
        int currentSpawnSide = 1; // Changed from float to int
        
        // Spawn initial planets at different heights with cascade pattern
        for (int i = 0; i < initialPlanets; i++)
        {
            float heightOffset = initialSpawnHeight + (i * verticalSpacing); // Regular vertical spacing
            
            // Calculate X position based on cascade pattern
            float xOffset = CalculateCascadeXOffset(camWidth, currentSpawnSide);
            float xPos = middleX + xOffset;
            
            // Alternate sides for zigzag or follow direction
            currentSpawnSide = GetNextSpawnSide(currentSpawnSide);
            
            Vector3 spawnPos = new Vector3(xPos, heightOffset, 0);

            GameObject newPlanet = Instantiate(planetPrefab, spawnPos, Quaternion.identity);

            Planet planetComponent = newPlanet.GetComponent<Planet>();
            if (planetComponent != null)
            {
                // Use values directly without multiplier
                float randomSize = Random.Range(minPlanetSize, maxPlanetSize);
                float speed = Random.Range(10f, 30f);
                float fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);

                // Select a random texture
                Sprite randomTexture = null;
                if (planetTextures != null && planetTextures.Count > 0)
                {
                    randomTexture = planetTextures[Random.Range(0, planetTextures.Count)];
                }

                newPlanet.tag = "Planet";
                planetComponent.Initialize(speed, randomSize, fallSpeed, randomTexture);
            }

            planets.Add(newPlanet);
        }
    }

    void Update()
    {
        if (!gameStarted) return;

        // Clean up the list of destroyed planets
        planets.RemoveAll(planet => planet == null);

        // Always have a certain number of active planets
        if (planets.Count < maxPlanets)
        {
            SpawnPlanet();
        }
    }

    public void StartGame()
    {
        gameStarted = true;

        // Spawn a new planet so the player has a destination
        while (planets.Count < maxPlanets)
        {
            SpawnPlanet();
        }
    }

    // Determines which side to spawn the next planet
    private int GetNextSpawnSide(int currentSide)
    {
        // Convert float to int for decision
        if (cascadeDirection < -0.1f) // Cascade to the left
            return -1;
        else if (cascadeDirection > 0.1f) // Cascade to the right
            return 1;
        else // Zigzag (alternate sides)
            return -currentSide;
    }

    // Calculate X offset for cascade effect
    private float CalculateCascadeXOffset(float camWidth, int spawnSide)
    {
        // Base value that determines horizontal amplitude
        float baseWidth = camWidth * 0.3f * cascadeIntensity;
        
        // Add some random variation
        float randomVariation = Random.Range(-0.2f, 0.2f) * baseWidth;
        
        return (baseWidth + randomVariation) * spawnSide;
    }

    Vector3 GetValidSpawnPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return Vector3.zero;

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        float middleX = cam.transform.position.x;

        // Spawn height above camera
        float spawnY = cam.transform.position.y + camHeight / 2 + spawnHeightAboveCamera;

        // Ensure minimum distance from last spawned planet
        if (Mathf.Abs(spawnY - lastSpawnY) < verticalSpacing)
        {
            spawnY = lastSpawnY + verticalSpacing;
        }

        // Calculate X position based on cascade pattern
        float xOffset = CalculateCascadeXOffset(camWidth, lastSpawnDirection);
        float spawnX = middleX + xOffset;
        
        // Update direction for next spawn
        lastSpawnDirection = GetNextSpawnSide(lastSpawnDirection);

        // Limit X position within screen bounds
        float minX = middleX - camWidth / 2 + 2;
        float maxX = middleX + camWidth / 2 - 2;
        spawnX = Mathf.Clamp(spawnX, minX, maxX);

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);

        // Check if position is too close to other planets
        for (int attempt = 0; attempt < 5; attempt++)
        {
            bool tooClose = false;
            foreach (GameObject planet in planets)
            {
                if (planet != null && Vector3.Distance(spawnPosition, planet.transform.position) < minSpawnDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                lastSpawnY = spawnY;
                return spawnPosition;
            }

            // If too close, adjust position
            spawnY += 1f;
            xOffset = CalculateCascadeXOffset(camWidth, lastSpawnDirection) * (0.8f - attempt * 0.1f);
            spawnX = middleX + xOffset;
            spawnX = Mathf.Clamp(spawnX, minX, maxX);
            spawnPosition = new Vector3(spawnX, spawnY, 0);
        }

        // Fallback position
        lastSpawnY = spawnY;
        return spawnPosition;
    }

    void SpawnPlanet()
    {
        // Don't spawn if existing planets are too close to each other
        foreach (GameObject planet in planets)
        {
            if (planet == null) continue;
            foreach (GameObject otherPlanet in planets)
            {
                if (otherPlanet == null || planet == otherPlanet) continue;
                if (Vector3.Distance(planet.transform.position, otherPlanet.transform.position) < minSpawnDistance * 0.5f)
                {
                    return; // Skip this spawn
                }
            }
        }

        Vector3 spawnPosition = GetValidSpawnPosition();
        GameObject newPlanet = Instantiate(planetPrefab, spawnPosition, Quaternion.identity);

        Planet planetComponent = newPlanet.GetComponent<Planet>();
        if (planetComponent != null)
        {
            // Use values directly without multiplier
            float randomSize = Random.Range(minPlanetSize, maxPlanetSize);
            float speed = Random.Range(10f, 30f);

            // Set a random fall speed in the configured range
            float fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);

            // Select a random texture if available
            Sprite randomTexture = null;
            if (planetTextures != null && planetTextures.Count > 0)
            {
                randomTexture = planetTextures[Random.Range(0, planetTextures.Count)];

                // Make sure the new planet has a SpriteRenderer
                SpriteRenderer renderer = newPlanet.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = newPlanet.AddComponent<SpriteRenderer>();
                }

                // Make sure the renderer is properly configured
                renderer.sortingOrder = 1;
            }

            // Make sure the planet has the right tag
            newPlanet.tag = "Planet";

            // Initialize the planet with all parameters
            planetComponent.Initialize(speed, randomSize, fallSpeed, randomTexture);

            // If the game is already in progress and first jump made, make it fall immediately
            if (gameStarted && PlayerController.HasJumped)
            {
                planetComponent.StartFalling();
            }
        }

        planets.Add(newPlanet);
    }

    // This method is called by GameManager when the player makes their first jump
    public void OnPlayerFirstJump()
    {
        // Activate descent of ALL planets
        foreach (GameObject planet in planets)
        {
            if (planet != null)
            {
                Planet planetScript = planet.GetComponent<Planet>();
                if (planetScript != null)
                {
                    planetScript.StartFalling();
                }
            }
        }

        // Set HasJumped for new planets
        PlayerController.SetHasJumped(true);
    }
}

