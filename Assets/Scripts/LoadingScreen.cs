using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public Slider progressBar;
    public float loadSpeed = 0.2f; // Adjust for smoother/slower effect

    void Start()
    {
        StartCoroutine(LoadInstructionsSceneAsync());
    }

    IEnumerator LoadInstructionsSceneAsync()
    {
        string sceneName = "InstructionsScene"; // Load Instructions Scene first

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Prevent auto-transition

        float progressValue = 0f;

        while (!operation.isDone)
        {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Smoothly fill the progress bar
            while (progressValue < targetProgress)
            {
                progressValue += Time.deltaTime * loadSpeed;
                progressBar.value = progressValue;
                yield return null;
            }

            // Ensure progress reaches 100% before activating Instructions Scene
            if (progressValue >= 1f && operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1f);
                operation.allowSceneActivation = true; // Now activate InstructionsScene
            }

            yield return null;
        }
    }
}
