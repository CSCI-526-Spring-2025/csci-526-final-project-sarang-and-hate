using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    void Awake()
    {
        // If the game is already in LoadingScene, do nothing
        if (SceneManager.GetActiveScene().name == "LoadingScene")
        {
            return;
        }

        // If the game is already in InstructionsScene or SampleScene, do nothing
        if (SceneManager.GetActiveScene().name == "InstructionsScene" || SceneManager.GetActiveScene().name == "SampleScene" || SceneManager.GetActiveScene().name == "Scene2")
        {
            return;
        }

        // Otherwise, force-load the LoadingScene at the beginning
        SceneManager.LoadScene("LoadingScene");
    }
}
