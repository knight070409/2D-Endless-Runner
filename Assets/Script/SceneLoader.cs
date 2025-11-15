using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "Game";


    // Load Game Scene
    public void LoadGameScene()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(gameSceneName);
    }

    // Quit Game
    public void QuitGame()
    {
        Application.Quit();
    }
}