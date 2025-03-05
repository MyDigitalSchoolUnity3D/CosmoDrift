using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;       // Link to your Start Button
    public Text scoreText;           // Link to your Score Text
    public GameObject gameOverPanel; // Link to your GameOverPanel

    [Header("Game Settings")]
    public float scoreMultiplier = 1f;

    private bool gameRunning = false;
    private float gameScore = 0f;
    private Transform player;
    private PlanetSpawner planetSpawner;

    // Events
    public delegate void GameStateHandler();
    public static event GameStateHandler OnGameStart;
    public static event GameStateHandler OnGameOver;

    void Start()
    {
        // Trouver les références
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        planetSpawner = Object.FindFirstObjectByType<PlanetSpawner>();

        // Configurer l'interface
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Configurer le bouton de démarrage
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        // S'abonner aux événements
        PlayerController.OnFirstJump += OnFirstJump;
    }

    void OnDestroy()
    {
        // Nettoyage
        PlayerController.OnFirstJump -= OnFirstJump;
        if (startButton != null)
            startButton.onClick.RemoveListener(StartGame);
    }

    void Update()
    {
        if (gameRunning && player != null)
        {
            // Mise à jour du score
            UpdateScore();

            // Vérifier si le joueur est sorti de l'écran
            CheckGameOver();
        }

        // Raccourci clavier pour déboguer - Appuyer sur F pour forcer toutes les planètes à tomber
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Touche F pressée - Forçage de la chute des planètes");
            ForceAllPlanetsToFall();
        }
    }

    public void StartGame()
    {
        // Démarrer le jeu
        gameRunning = true;
        gameScore = 0;
        UpdateScoreDisplay();

        // Cacher le bouton de démarrage
        if (startButton != null)
            startButton.gameObject.SetActive(false);

        // Démarrer le spawner de planètes
        if (planetSpawner != null)
            planetSpawner.StartGame();

        // Notifier les observateurs
        if (OnGameStart != null)
            OnGameStart();

        Debug.Log("Game started by button press - Planets should not fall until first jump");
    }

    void OnFirstJump()
    {
        Debug.Log("🟢 GameManager: OnFirstJump reçu!");

        // Si le jeu n'est pas déjà démarré, le démarrer
        if (!gameRunning)
        {
            StartGame();
        }

        // Dire au spawner que le premier saut a eu lieu
        if (planetSpawner != null)
        {
            Debug.Log("🟢 GameManager: Appel de planetSpawner.OnPlayerFirstJump()");
            planetSpawner.OnPlayerFirstJump();
        }
        else
        {
            Debug.LogError("🔴 ERREUR: planetSpawner est null dans GameManager!");
        }
    }

    void UpdateScore()
    {
        // Le score est basé sur la hauteur
        float playerHeight = player.position.y;
        float newScore = Mathf.Max(gameScore, playerHeight * scoreMultiplier);

        if (newScore > gameScore)
        {
            gameScore = newScore;
            UpdateScoreDisplay();
        }
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Mathf.FloorToInt(gameScore);
        }
    }

    void CheckGameOver()
    {
        // Le joueur perd s'il sort de l'écran par le bas
        Vector3 screenPos = Camera.main.WorldToViewportPoint(player.position);
        if (screenPos.y < -0.2f)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        if (!gameRunning) return;

        gameRunning = false;

        // Afficher l'écran de game over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Afficher le score final
            Text finalScoreText = gameOverPanel.GetComponentInChildren<Text>();
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + Mathf.FloorToInt(gameScore);
            }
        }

        // Notifier les observateurs
        if (OnGameOver != null)
            OnGameOver();
    }

    public void RestartGame()
    {
        // Recharger la scène courante
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Méthode de débogage pour forcer la chute des planètes
    public void ForceAllPlanetsToFall()
    {
        Debug.Log("🔧 GameManager: Forçage de la chute de toutes les planètes");

        // Trouver toutes les planètes par tag
        GameObject[] allPlanets = GameObject.FindGameObjectsWithTag("Planet");

        foreach (GameObject planet in allPlanets)
        {
            // Ne pas faire tomber la planète de départ
            if (planetSpawner != null && planet == planetSpawner.startPlanet)
            {
                continue;
            }

            Planet planetScript = planet.GetComponent<Planet>();
            if (planetScript != null)
            {
                // Forcer la chute directement
                System.Reflection.FieldInfo field = typeof(Planet).GetField("gameStarted",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);

                if (field != null)
                {
                    field.SetValue(planetScript, true);
                    Debug.Log($"🔧 Planète {planet.name} forcée à tomber par reflection");
                }
            }
        }
    }
}
