using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuCanvas;
    public GameObject optionsCanvas;
    
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseGame();
        }
    }
    public void pauseGame()
    {
        optionsCanvas.SetActive(true);
        Time.timeScale = 0f; 
        
    }
    public void Resume()
    {
        optionsCanvas.SetActive(false);
        Time.timeScale = 1f;
    }
    public void LoadMap()
    {
        SceneManager.LoadScene(1); 
    }
    public void QuitGame()
    {
        Debug.Log("Quit Game"); 
        Application.Quit();
    }
    public void ShowOptions()
    {
        mainMenuCanvas.SetActive(false);
        optionsCanvas.SetActive(true);
    }

    public void BackToMain()
    {
        optionsCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
    }
    
}
