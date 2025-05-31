using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Call this when the Play button is clicked
    public void PlayGame()
    {
        // Replace "GameScene" with the actual name of your game scene
        SceneManager.LoadScene("GameScene");
    }

    // Call this when the Quit button is clicked
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();

        // If you're testing in the editor, stop play mode (only in Editor)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }    
}
