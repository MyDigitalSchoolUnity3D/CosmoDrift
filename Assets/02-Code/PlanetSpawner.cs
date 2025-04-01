using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlanetSpawner : MonoBehaviour
{
    public GameObject planetPrefab;
    public GameObject startPlanet;
    public int maxPlanets = 3;
    public float minSpawnDistance = 6f;
    public float minPlanetSize = 0.8f;
    public float maxPlanetSize = 1.5f;
    public Vector3 startPlanetInitialPosition; // à stocker au départ


    // Paramètres de vitesse de chute
    [Header("Fall Speed Configuration")]
    [Range(0.1f, 2.0f)] public float minFallSpeed = 0.3f;
    [Range(0.1f, 2.0f)] public float maxFallSpeed = 0.7f;

    private List<GameObject> planets = new List<GameObject>();
    private bool gameStarted = false;

    void Start()
{
    if (startPlanet != null)
    {
        startPlanetInitialPosition = startPlanet.transform.position;
        startPlanet.tag = "StartPlanet";
        planets.Add(startPlanet);
    }
}

public void ResetSpawner()
{
    foreach (GameObject planet in planets)
    {
        if (planet != null && planet != startPlanet)
        {
            Destroy(planet);
        }
    }

    planets.Clear();

    if (startPlanet != null)
    {
        startPlanet.transform.position = startPlanetInitialPosition;
        startPlanet.transform.rotation = Quaternion.identity;
        planets.Add(startPlanet);
    }

    gameStarted = false;
}
    void Update()
    {
        if (!gameStarted) return;

        // Nettoyer la liste des planètes détruites
        planets.RemoveAll(planet => planet == null);

        // Toujours avoir un certain nombre de planètes actives
        if (planets.Count < maxPlanets)
        {
            SpawnPlanet();
        }
    }

    public void StartGame()
    {
        gameStarted = true;

        // Spawn d'une nouvelle planète pour que le joueur ait une destination
        while (planets.Count < maxPlanets)
        {
            SpawnPlanet();
        }
    }

    public void OnPlayerFirstJump()
    {
        Debug.Log("🟢 PlanetSpawner: Premier saut détecté, les planètes commencent à tomber");

        int planetsCount = 0;
        int fallingPlanets = 0;

        // Activer la descente de TOUTES les planètes, y compris la planète de départ
        foreach (GameObject planet in planets)
        {
            if (planet != null)
            {
                planetsCount++;
                Debug.Log($"🟢 Demande à la planète {planet.name} de tomber");
                Planet planetScript = planet.GetComponent<Planet>();
                if (planetScript != null)
                {
                    planetScript.StartFalling();
                    fallingPlanets++;
                }
            }
        }

        Debug.Log($"🟢 Résumé: {planetsCount} planètes trouvées, {fallingPlanets} vont tomber");

        // Force-set HasJumped pour les nouvelles planètes
        PlayerController.SetHasJumped(true);
    }

    void SpawnPlanet()
    {
        Vector3 spawnPosition = GetValidSpawnPosition();
        GameObject newPlanet = Instantiate(planetPrefab, spawnPosition, Quaternion.identity);

        Planet planetComponent = newPlanet.GetComponent<Planet>();
        if (planetComponent != null)
        {
            // Générer une taille aléatoire seulement pour les nouvelles planètes
            float randomSize = Random.Range(minPlanetSize, maxPlanetSize);
            float speed = Random.Range(10f, 30f);

            // Définir une vitesse de chute aléatoire dans la plage configurée
            float fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);

            // Initialiser la planète avec tous les paramètres
            planetComponent.Initialize(speed, randomSize, fallSpeed);

            // Si le jeu est déjà en cours et premier saut fait, faire tomber immédiatement
            if (gameStarted && PlayerController.HasJumped)
            {
                planetComponent.StartFalling();
            }
        }

        planets.Add(newPlanet);
    }

    Vector3 GetValidSpawnPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return Vector3.zero;

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // Zone de spawn - Forcer le spawn dans la caméra
        float minX = cam.transform.position.x - camWidth / 2 + 2;
        float maxX = cam.transform.position.x + camWidth / 2 - 2;
        float minY = cam.transform.position.y;
        float maxY = cam.transform.position.y + camHeight / 2 - 1;

        for (int attempt = 0; attempt < 15; attempt++)
        {
            float spawnX = Random.Range(minX, maxX);
            float spawnY = Random.Range(minY, maxY);
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);

            bool validPosition = true;
            foreach (GameObject planet in planets)
            {
                if (planet != null && Vector3.Distance(spawnPosition, planet.transform.position) < minSpawnDistance)
                {
                    validPosition = false;
                    break;
                }
            }

            if (validPosition)
            {
                return spawnPosition;
            }
        }

        // Position par défaut
        return new Vector3(
            cam.transform.position.x + Random.Range(-2f, 2f),
            cam.transform.position.y + camHeight / 3,
            0
        );
    }
}
