using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Build index of the scene to load")]
    [SerializeField] private int sceneBuildIndex;

    public void PlayGame()
    {
        if (sceneBuildIndex >= 0)
        {
            SceneManager.LoadScene(sceneBuildIndex);
        }
        else
        {
            Debug.LogError("Invalid Scene Build Index!");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}