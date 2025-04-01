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
        // Find references
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        planetSpawner = Object.FindFirstObjectByType<PlanetSpawner>();

        // Configure interface
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Configure start button
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        // Subscribe to events
        PlayerController.OnFirstJump += OnFirstJump;
    }

    void OnDestroy()
    {
        // Cleanup
        PlayerController.OnFirstJump -= OnFirstJump;
        if (startButton != null)
            startButton.onClick.RemoveListener(StartGame);
    }

    void Update()
    {
        if (gameRunning && player != null)
        {
            // Update score
            UpdateScore();

            // Check if player is off screen
            CheckGameOver();
        }

        // Keyboard shortcut for debugging - Press F to force all planets to fall
        if (Input.GetKeyDown(KeyCode.F))
        {
            ForceAllPlanetsToFall();
        }
    }

    public void StartGame()
    {
        // Start the game
        gameRunning = true;
        gameScore = 0;
        UpdateScoreDisplay();

        // Hide the start button
        if (startButton != null)
            startButton.gameObject.SetActive(false);

        // Start the planet spawner
        if (planetSpawner != null)
            planetSpawner.StartGame();

        // Notify observers
        if (OnGameStart != null)
            OnGameStart();
    }

    void OnFirstJump()
    {
        // If the game isn't already started, start it
        if (!gameRunning)
        {
            StartGame();
        }

        // Tell the spawner that the first jump has occurred
        if (planetSpawner != null)
        {
            planetSpawner.OnPlayerFirstJump();
        }
    }

    void UpdateScore()
    {
        // Score is based on height
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
        // Player loses if they go off screen at the bottom
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

        // Show the game over screen
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Display the final score
            Text finalScoreText = gameOverPanel.GetComponentInChildren<Text>();
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + Mathf.FloorToInt(gameScore);
            }
        }

        // Notify observers
        if (OnGameOver != null)
            OnGameOver();
    }

    public void RestartGame()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Debug method to force planets to fall
    public void ForceAllPlanetsToFall()
    {
        // Find all planets by tag
        GameObject[] allPlanets = GameObject.FindGameObjectsWithTag("Planet");

        foreach (GameObject planet in allPlanets)
        {
            // Don't make the starting planet fall
            if (planetSpawner != null && planet == planetSpawner.startPlanet)
            {
                continue;
            }

            Planet planetScript = planet.GetComponent<Planet>();
            if (planetScript != null)
            {
                // Force the fall directly
                System.Reflection.FieldInfo field = typeof(Planet).GetField("gameStarted",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);

                if (field != null)
                {
                    field.SetValue(planetScript, true);
                }
            }
        }
    }

    public void AddScoreBonus(float bonus)
    {
        gameScore += bonus;
        UpdateScoreDisplay();
    }
}
