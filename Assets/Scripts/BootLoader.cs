// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class BootLoader : MonoBehaviour
// {
//     void Awake()
//     {
//         // If the game is starting directly in SampleScene (not from LoadingScene)
//         if (SceneManager.GetActiveScene().name == "SampleScene" && !HasLoadingHappened())
//         {
//             SceneManager.LoadScene("LoadingScene");
//         }
//     }

//     // Function to check if LoadingScene has already happened
//     private bool HasLoadingHappened()
//     {
//         return PlayerPrefs.GetInt("LoadingCompleted", 0) == 1;
//     }
// }


using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    void Awake()
    {
        // Reset the loading state when the game starts
        if (!PlayerPrefs.HasKey("GameStarted"))
        {
            PlayerPrefs.DeleteKey("LoadingCompleted"); // Reset the loading state
            PlayerPrefs.SetInt("GameStarted", 1); // Mark that the game has started
            PlayerPrefs.Save();
        }

        // If we are in SampleScene and loading hasn't been done, go to LoadingScene
        if (SceneManager.GetActiveScene().name == "SampleScene" && !HasLoadingHappened())
        {
            SceneManager.LoadScene("LoadingScene");
        }
    }

    // Function to check if LoadingScene has already happened
    private bool HasLoadingHappened()
    {
        return PlayerPrefs.GetInt("LoadingCompleted", 0) == 1;
    }
}


