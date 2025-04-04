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

        planetSpawner = FindFirstObjectByType<PlanetSpawner>();
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
        try
        {
            if (playerController == null || planetSpawner == null || planetSpawner.startPlanet == null)
                return;

            HideMenuUI();
            scoreButton.gameObject.SetActive(true);
            playerController.ResetPlayer(planetSpawner.startPlanet.transform);
            gameRunning = true;
            gameScore = 0;
            UpdateScoreDisplay();
            planetSpawner.StartGame(startPlanetFalls: false);

            // Marquer que le jeu a commencé
            PlayerController.SetHasJumped(false);  // Le premier saut n'a pas encore eu lieu
            
        }
        catch (System.Exception e)
        {            
            // Reset le jeu si system.exception
            gameStartedOnce = false;
            ShowMenuUI();
            scoreButton.gameObject.SetActive(false);
        }
    }

    void CheckPlayerOutOfBounds()
    {
        if (player == null || Camera.main == null)
        {
            return;
        }
        
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
        // 🔁 Réinitialiser état du jeu
        gameRunning = false;
        gameStartedOnce = false;
        gameScore = 0;
        UpdateScoreDisplay();

        // Réinitialiser les planètes en premier (pour recréer la planète de départ si nécessaire)
        if (planetSpawner != null)
            planetSpawner.ResetSpawner();

        // Vérifier que la planète de départ existe maintenant
        if (planetSpawner != null && planetSpawner.startPlanet != null)
        {
            // Réinitialiser le joueur seulement si on a la planète de départ
            if (playerController != null)
            {
                playerController.ResetPlayer(planetSpawner.startPlanet.transform);
            }
           
        }

        PlayerController.SetHasJumped(false);

        // Réinitialiser l’UI
        scoreButton.gameObject.SetActive(false);
        ShowMenuUI();

        // Reconnecter le bouton Start (au cas où)
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
