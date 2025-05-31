using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoLoadNextScene : MonoBehaviour
{
    void Start()
    {
        // Load the next scene based on build index
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        // Make sure the next scene index is valid
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {  
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        { 
            Debug.LogWarning("No next scene found. Make sure to add more scenes to Build Settings.");
        }
    }
}
