using UnityEngine;
using System.Collections.Generic;

public class PlanetSpawner : MonoBehaviour
{
    // References
    public GameObject planetPrefab;
    public GameObject startPlanet;
    
    // Configuration générale
    [Header("Configuration générale")]
    public int maxPlanets = 3;
    public float minSpawnDistance = 5f;
    public float minPlanetSize = 0.8f;
    public float maxPlanetSize = 1.5f;
    [Range(0.1f, 2.0f)] public float minFallSpeed = 0.2f;
    [Range(0.1f, 2.0f)] public float maxFallSpeed = 1.2f;
    
    // Configuration style arcade
    [Header("Configuration Arcade")]
    public bool arcadeMode = true;
    [Range(1, 3)] public int columns = 3;
    [Range(2, 6)] public float verticalSpacing = 4f;
    [Range(0f, 1f)] public float horizontalVariation = 0.2f;
    public float initialHeight = 5f;
    
    // Textures
    [Header("Textures")]
    public List<Sprite> planetTextures = new List<Sprite>();
    
    // Variables privées
    private List<GameObject> planets = new List<GameObject>();
    private Vector3 startPlanetInitialPosition;
    private bool gameStarted = false;
    private Camera mainCamera;
    private int spawnCounter = 0;

    private void Awake() {
        // Cache la référence à la caméra principale
        mainCamera = Camera.main;
    }

    void Start()
    {
        if (startPlanet != null)
        {
            startPlanetInitialPosition = startPlanet.transform.position;
            startPlanet.tag = "StartPlanet";
            planets.Add(startPlanet);
        }
    }

    void Update()
    {
        if (!gameStarted) return;

        // Enlever les planètes nulles (détruites)
        for (int i = planets.Count - 1; i >= 0; i--)
        {
            if (planets[i] == null)
                planets.RemoveAt(i);
        }

        // Maintenir le nombre de planètes
        while (planets.Count < maxPlanets)
        {
            SpawnPlanet();
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        spawnCounter = 0;
        
        // Générer les planètes initiales
        for (int i = 0; i < maxPlanets; i++)
        {
            SpawnPlanet();
        }
    }

    public void OnPlayerFirstJump()
    {
        for (int i = 0; i < planets.Count; i++)
        {
            GameObject planet = planets[i];
            if (planet != null && planet != startPlanet)
            {
                planet.GetComponent<Planet>()?.StartFalling();
            }
        }
    }

    void SpawnPlanet()
    {
        Vector3 spawnPosition = GetArcadeSpawnPosition();
        
        GameObject newPlanet = Instantiate(planetPrefab, spawnPosition, Quaternion.identity);
        Planet planetComponent = newPlanet.GetComponent<Planet>();
        
        if (planetComponent != null)
        {
            float size = Random.Range(minPlanetSize, maxPlanetSize);
            float rotationSpeed = Random.Range(10f, 30f);
            float fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);
            
            Sprite texture = planetTextures.Count > 0 
                ? planetTextures[Random.Range(0, planetTextures.Count)] 
                : null;
            
            newPlanet.tag = "Planet";
            planetComponent.Initialize(rotationSpeed, size, fallSpeed, texture);
            
            // Faire tomber immédiatement si le jeu est déjà en cours
            if (gameStarted && PlayerController.HasJumped)
            {
                planetComponent.StartFalling();
            }
            
            planets.Add(newPlanet);
        }
    }

    Vector3 GetArcadeSpawnPosition()
    {
        if (!mainCamera) mainCamera = Camera.main;
        if (!mainCamera) return new Vector3(0, 10, 0);  // Fallback
        
        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;
        float screenTop = mainCamera.transform.position.y + camHeight/2;
        
        // Position verticale au-dessus de l'écran
        float spawnY = screenTop + 3f;
        
        // En mode arcade: diviser l'écran en colonnes
        float columnWidth = camWidth / columns;
        int column = spawnCounter % columns;
        spawnCounter++;
        
        // Position horizontale au centre de la colonne + variation aléatoire
        float midX = mainCamera.transform.position.x - camWidth/2 + columnWidth * (column + 0.5f);
        float variation = columnWidth * horizontalVariation;
        float spawnX = midX + Random.Range(-variation, variation);
        
        // Vérifier que la nouvelle position n'est pas trop proche d'une planète existante
        Vector3 position = new Vector3(spawnX, spawnY, 0);
        
        // S'il y a collision, ajuster vers le haut
        int safetyCounter = 0;
        while (IsTooCloseToOtherPlanets(position) && safetyCounter < 10)
        {
            position.y += verticalSpacing / 2;
            safetyCounter++;
        }
        
        return position;
    }
    
    bool IsTooCloseToOtherPlanets(Vector3 position)
    {
        foreach (GameObject planet in planets)
        {
            if (planet == null) continue;
            if (Vector3.Distance(position, planet.transform.position) < minSpawnDistance)
            {
                return true;
            }
        }
        return false;
    }

    public void ResetSpawner()
    {
        // Supprimer toutes les planètes sauf la planète de départ
        for (int i = planets.Count - 1; i >= 0; i--)
        {
            GameObject planet = planets[i];
            if (planet != null && planet != startPlanet)
            {
                Destroy(planet);
                planets.RemoveAt(i);
            }
        }
        
        // Vider la liste et réajouter la planète de départ
        planets.Clear();
        
        if (startPlanet != null)
        {
            startPlanet.transform.position = startPlanetInitialPosition;
            startPlanet.transform.rotation = Quaternion.identity;
            planets.Add(startPlanet);
        }
        
        gameStarted = false;
        spawnCounter = 0;
    }
}

