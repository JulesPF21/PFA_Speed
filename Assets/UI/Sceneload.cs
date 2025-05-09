using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Sceneload : MonoBehaviour
{
    // This method will be called when the button is clicked
    public void LoadDemoScene()
    {
        SceneManager.LoadScene(sceneBuildIndex: 1);
    }
}