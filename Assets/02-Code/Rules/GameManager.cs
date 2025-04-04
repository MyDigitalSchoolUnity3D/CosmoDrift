using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public Button leaveButton;
    public Button scoreButton;
    public Button bestScoreTextButton;
    public GameObject bestScoreDecor;

    [Header("Game Settings")]
    public float scoreMultiplier = 1f;

    [Header("Difficulté")]
    public float difficulty = 1f;
    public float difficultyIncreaseRate = 0.05f;
    public float maxDifficulty = 5f;

    private bool gameRunning = false;
    private bool gameStartedOnce = false;
    private float gameScore = 0f;
    private int bestScore = 0;

    private Transform player;
    private PlayerController playerController;
    private PlanetSpawner planetSpawner;
    private BackgroundManager backgroundManager;

    // Valeurs d'origine à restaurer au reset
    private int initialMaxPlanets;
    private float initialMinFallSpeed;
    private float initialMaxFallSpeed;
    private float initialSpawnInterval;
    private float initialBackgroundScrollSpeed;

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = player.GetComponent<PlayerController>();
        }

        planetSpawner = FindFirstObjectByType<PlanetSpawner>();
        backgroundManager = FindFirstObjectByType<BackgroundManager>();

        if (planetSpawner != null)
        {
            initialMaxPlanets = planetSpawner.maxPlanets;
            initialMinFallSpeed = planetSpawner.minFallSpeed;
            initialMaxFallSpeed = planetSpawner.maxFallSpeed;
            initialSpawnInterval = planetSpawner.spawnInterval;
        }

        if (backgroundManager != null)
        {
            initialBackgroundScrollSpeed = backgroundManager.scrollSpeed;
        }

        startButton.onClick.AddListener(TriggerStart);
        leaveButton.onClick.AddListener(QuitGame);

        ShowMenuUI();
        scoreButton.gameObject.SetActive(false);

        LoadBestScore();
    }

    void Update()
    {
        if (!gameRunning && !gameStartedOnce && Input.GetKeyDown(KeyCode.Space))
        {
            TriggerStart();
        }

        if (gameRunning && player != null)
        {
            UpdateScore();
            UpdateDifficulty();
            CheckPlayerOutOfBounds();
        }
    }

    public void TriggerStart()
    {
        if (gameStartedOnce || playerController == null || planetSpawner == null)
            return;

        gameStartedOnce = true;
        StartGame();
    }

    private void StartGame()
    {
        try
        {
            if (playerController == null || planetSpawner == null || planetSpawner.startPlanet == null)
                return;

            HideMenuUI();
            scoreButton.gameObject.SetActive(true);

            // Réinitialisation
            gameScore = 0f;
            difficulty = 1f;
            gameRunning = true;

            planetSpawner.ResetSpawner();
            planetSpawner.maxPlanets = initialMaxPlanets;
            planetSpawner.minFallSpeed = initialMinFallSpeed;
            planetSpawner.maxFallSpeed = initialMaxFallSpeed;
            planetSpawner.spawnInterval = initialSpawnInterval;

            if (backgroundManager != null)
                backgroundManager.scrollSpeed = initialBackgroundScrollSpeed;

            planetSpawner.StartGame(startPlanetFalls: false);
            playerController.ResetPlayer(planetSpawner.startPlanet.transform);

            PlayerController.SetHasJumped(false);
            UpdateScoreDisplay();
        }
        catch (System.Exception e)
        {
            gameStartedOnce = false;
            ShowMenuUI();
            scoreButton.gameObject.SetActive(false);
            Debug.LogError("❌ Erreur au démarrage : " + e.Message);
        }
    }

    void CheckPlayerOutOfBounds()
    {
        if (player == null || Camera.main == null)
            return;

        Vector3 viewPos = Camera.main.WorldToViewportPoint(player.position);
        if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1)
        {
            SaveBestScore();
            gameRunning = false;
            ReturnToMenu();
        }
    }

    void ReturnToMenu()
    {
        gameRunning = false;
        gameStartedOnce = false;
        gameScore = 0f;
        difficulty = 1f;

        UpdateScoreDisplay();

        if (planetSpawner != null)
        {
            planetSpawner.ResetSpawner();

            // Rétablir les valeurs par défaut
            planetSpawner.maxPlanets = initialMaxPlanets;
            planetSpawner.minFallSpeed = initialMinFallSpeed;
            planetSpawner.maxFallSpeed = initialMaxFallSpeed;
            planetSpawner.spawnInterval = initialSpawnInterval;
        }

        if (backgroundManager != null)
        {
            backgroundManager.scrollSpeed = initialBackgroundScrollSpeed;
        }

        if (planetSpawner != null && planetSpawner.startPlanet != null && playerController != null)
        {
            playerController.ResetPlayer(planetSpawner.startPlanet.transform);
        }

        PlayerController.SetHasJumped(false);

        scoreButton.gameObject.SetActive(false);
        ShowMenuUI();

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(TriggerStart);
    }

    void ShowMenuUI()
    {
        startButton.gameObject.SetActive(true);
        leaveButton.gameObject.SetActive(true);
        bestScoreTextButton.gameObject.SetActive(true);
        bestScoreDecor.SetActive(true);
    }

    void HideMenuUI()
    {
        startButton.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
        bestScoreTextButton.gameObject.SetActive(false);
        bestScoreDecor.SetActive(false);
    }

    void UpdateScore()
    {
        gameScore += Time.deltaTime * scoreMultiplier;
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        TMP_Text scoreText = scoreButton.GetComponentInChildren<TMP_Text>();
        if (scoreText != null)
        {
            scoreText.text = Mathf.FloorToInt(gameScore).ToString();
        }
    }

    void UpdateDifficulty()
    {
        difficulty += Time.deltaTime * difficultyIncreaseRate;
        difficulty = Mathf.Clamp(difficulty, 1f, maxDifficulty);

        if (planetSpawner != null)
        {
    
            planetSpawner.minFallSpeed = initialMinFallSpeed * difficulty;
            planetSpawner.maxFallSpeed = initialMaxFallSpeed * difficulty;
        }

        if (backgroundManager != null)
        {
            backgroundManager.scrollSpeed = initialBackgroundScrollSpeed + difficulty * 1f;
        }
    }

    void LoadBestScore()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        TMP_Text text = bestScoreTextButton.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = bestScore.ToString();
        }
    }

    void SaveBestScore()
    {
        if (gameScore > bestScore)
        {
            bestScore = Mathf.FloorToInt(gameScore);
            PlayerPrefs.SetInt("BestScore", bestScore);

            TMP_Text text = bestScoreTextButton.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = bestScore.ToString();
            }
        }
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
