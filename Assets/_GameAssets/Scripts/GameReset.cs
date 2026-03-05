using UnityEngine;
using UnityEngine.SceneManagement;

public class GameReset : MonoBehaviour
{
    /// <summary>
    /// Restarts the game, destroys the CoreGameManager instance so it will reinitialize.
    /// </summary>
    public void RestartGame()
    {
        // Destroy the CoreGameManager singleton if it exists
        if (CoreGameManager.Instance != null)
        {
            Destroy(CoreGameManager.Instance.gameObject);
        }

        // Reload the active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
