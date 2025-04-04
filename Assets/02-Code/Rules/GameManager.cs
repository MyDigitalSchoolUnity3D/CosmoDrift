using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    private bool gameRunning = false;
    private bool gameStartedOnce = false; // Pour éviter plusieurs déclenchements
    private float gameScore = 0f;
    private int bestScore = 0;

    private Transform player;
    private PlayerController playerController;
    private PlanetSpawner planetSpawner;

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = player.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogError("❌ Aucun objet avec le tag 'Player' trouvé !");
        }

        planetSpawner = FindFirstObjectByType<PlanetSpawner>();
        if (planetSpawner == null)
        {
            Debug.LogError("❌ Aucun PlanetSpawner trouvé dans la scène !");
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
            // Espace fait le même effet que le bouton start
            TriggerStart();
        }

        if (gameRunning && player != null)
        {
            UpdateScore();
            CheckPlayerOutOfBounds();
        }
    }

    public void TriggerStart()
    {
        // Si le jeu a déjà commencé, on ne relance pas
        if (gameStartedOnce || playerController == null || planetSpawner == null)
            return;

        gameStartedOnce = true;
        StartGame();
    }

    private void StartGame()
    {
        try {
            Debug.Log("🔄 Starting game sequence...");
            
            // Reset the player first, before setting game flags
            if (playerController == null)
            {
                Debug.LogError("❌ playerController is null!");
                return;
            }
            
            if (planetSpawner == null)
            {
                Debug.LogError("❌ planetSpawner is null!");
                return;
            }
            
            if (planetSpawner.startPlanet == null)
            {
                Debug.LogError("❌ startPlanet is null!");
                return;
            }
            
            // First just update UI to show we're starting
            HideMenuUI();
            scoreButton.gameObject.SetActive(true);
            
            // Now reset the player
            Debug.Log("🔄 Resetting player position...");
            playerController.ResetPlayer(planetSpawner.startPlanet.transform);
            
            // Update game state
            gameRunning = true;
            gameScore = 0;
            UpdateScoreDisplay();
            
            // Start planets AFTER player is reset
            Debug.Log("🔄 Starting planet spawner...");
            try {
                planetSpawner.StartGame();
            }
            catch (System.Exception e) {
                Debug.LogError("❌ Error in planetSpawner.StartGame(): " + e.ToString());
                throw; // Rethrow to be caught by outer try/catch
            }
            
            // Important: Call this method to make planets fall
            Debug.Log("🔄 Making planets start falling...");
            planetSpawner.OnPlayerFirstJump();
            
            // Set jump flag 
            PlayerController.SetHasJumped(true);
            
            Debug.Log("✅ Game started successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Error starting game: " + e.ToString());
            // Full error details
            Debug.LogException(e);
            
            // Reset game state if we hit an error
            gameStartedOnce = false;
            
            // Show UI again
            ShowMenuUI();
            scoreButton.gameObject.SetActive(false);
        }
    }

    void CheckPlayerOutOfBounds()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(player.position);
        if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1)
        {
            Debug.Log("💥 Le joueur est sorti de l’écran !");
            SaveBestScore();
            gameRunning = false;
            ReturnToMenu();
        }
    }

    void ReturnToMenu()
    {
        Debug.Log("↩️ Retour au menu demandé");

        // 🔁 Réinitialiser état du jeu
        gameRunning = false;
        gameStartedOnce = false;
        gameScore = 0;
        UpdateScoreDisplay();

        // 🔁 Réinitialiser les planètes
        if (planetSpawner != null)
            planetSpawner.ResetSpawner();

        // 🔁 Réinitialiser le joueur
        if (playerController != null && planetSpawner != null)
        {
            playerController.ResetPlayer(planetSpawner.startPlanet.transform);
        }

        PlayerController.SetHasJumped(false);

        // 🔁 Réinitialiser l’UI
        scoreButton.gameObject.SetActive(false);
        ShowMenuUI();

        // 🔁 Reconnecter le bouton Start (au cas où)
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(TriggerStart);

        Debug.Log("✅ Tout est prêt pour un nouveau départ");
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
        TMP_Text scoreText = scoreButton.GetComponentInChildren<TMP_Text>();
        if (scoreText != null)
        {
            scoreText.text = Mathf.FloorToInt(gameScore).ToString();
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
        Debug.Log("🛑 Quitter le jeu");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
