using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement player;
    [SerializeField] private LevelSpawner levelSpawner;
    [SerializeField] private BackgroundSpawner backgroundSpawner;

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main";

    private bool isGameOver;
    private bool isPaused;
    private int totalCoins;

    private const string TOTAL_COINS_KEY = "TotalCoins";

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        isGameOver = false;
        isPaused = false;

        totalCoins = PlayerPrefs.GetInt(TOTAL_COINS_KEY, 100);
        UpdateCoinUI();
    }

    void Update()
    {
        // Pause with ESC key for testing
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f; // Pause the game
    }

    public void PauseGame()
    {
        if (isGameOver || isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        isPaused = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (player != null)
            player.ResetPlayer();

        if (levelSpawner != null)
            levelSpawner.ResetSpawner();

        if (backgroundSpawner != null)
        {
            backgroundSpawner.ResetSpawner();
        }
    }

    public void HomeGame()
    {
        Time.timeScale = 1f; // Reset time scale in case game was paused
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void AddCoin()
    {
        totalCoins++;

        PlayerPrefs.SetInt(TOTAL_COINS_KEY, totalCoins);
        PlayerPrefs.Save();
        UpdateCoinUI();
    }

    private void UpdateCoinUI()
    {
        coinText.text = totalCoins.ToString();
    }

    //for testing
    public void ResetAllCoins()
    {
        totalCoins = 0;
        PlayerPrefs.SetInt(TOTAL_COINS_KEY, 0);
        PlayerPrefs.Save();
        UpdateCoinUI();
    }
}