using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadPreviousSceneOnStart : MonoBehaviour
{
    void Start()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = currentSceneIndex - 1;

        // Wrap around if current scene is the first one
        if (previousSceneIndex < 0)
        {
            previousSceneIndex = SceneManager.sceneCountInBuildSettings - 1;
        }

        SceneManager.LoadScene(previousSceneIndex);
    }
}
     