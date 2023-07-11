using UnityEngine;

public class GameLoader : MonoBehaviour
{
    public GameObject gameManager;

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            Instantiate(gameManager);
            Logger.Log("Game manager created");
        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
