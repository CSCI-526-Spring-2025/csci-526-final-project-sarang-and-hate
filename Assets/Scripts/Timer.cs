using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    public float timeRemaining = 120f; // 2 minutes
    private bool timerIsRunning = false;
    public Text timerText; // UI text for timer display
    public GameObject victoryPanel;
    public GameObject restartPanel;
    public GameObject dimBackground; // Background dim panel
    public Text victoryText;
    public Button restartButton;
    private bool playerWon = false;

    // Help button and panel
    public GameObject helpPanel;
    public Button helpButton;
    public Button exitHelpButton; // Exit button for loading scene

    void Start()
    {
        // Ensure timerText is assigned
        if (timerText != null)
        {
            RectTransform rectTransform = timerText.GetComponent<RectTransform>();

            // Set anchor to Top Center
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f); // Keep the pivot at the top center

            // Adjust position (move down from top)
            rectTransform.anchoredPosition = new Vector2(0, -50); // Adjust this value if needed

            Debug.Log("Timer UI anchored to the top center.");
        }
        else
        {
            Debug.LogError("timerText is not assigned in the Inspector!");
        }

        timerIsRunning = true;
        UpdateTimeDisplay();

        // Help button setup
        if (helpButton != null)
        {
            helpButton.onClick.AddListener(OpenHelp);
        }

        if (exitHelpButton != null)
        {
            exitHelpButton.onClick.AddListener(ExitToLoadingScene);
        }

        if (helpPanel != null)
        {
            helpPanel.SetActive(false); // Hide help panel initially
        }
    }

    void Update()
    {
        if (!timerIsRunning || playerWon)
            return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimeDisplay();
        }
        else
        {
            timeRemaining = 0;
            timerIsRunning = false;
            UpdateTimeDisplay();
            ShowRestartPanel();
        }
    }

    private void UpdateTimeDisplay()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timeRemaining);
            timerText.text = "Time Remaining: " + seconds.ToString() + "s";
        }
        else
        {
            Debug.LogError("TimerText UI is not assigned in the Inspector!");
        }
    }

    public void checkWinCondition()
    {
        if (timeRemaining > 0)
        {
            timerIsRunning = false;
            playerWon = true;
            ShowVictoryPanel();
        }
    }

    public void ShowVictoryPanel()
    {
        Debug.Log("🎉 Victory! Player completed the maze in time.");
        if (dimBackground != null)
        {
            dimBackground.SetActive(true); // Dim background
        }
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        if (victoryText != null)
        {
            victoryText.text = "Victory!";
            victoryText.color = Color.green;
        }
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
        Time.timeScale = 0;
    }

    private void ShowRestartPanel()
    {
        Debug.Log("⏳ Time's Up! Player failed to complete the maze in time.");
        if (dimBackground != null)
        {
            dimBackground.SetActive(true); // Show background dim effect
        }
        if (restartPanel != null)
        {
            restartPanel.SetActive(true);
        }
        if (victoryText != null)
        {
            victoryText.text = "Time Over!";
            victoryText.color = Color.red;
        }
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Help panel methods
    public void OpenHelp()
    {
        if (helpPanel != null)
        {
            helpPanel.SetActive(true);
        }
    }

    public void ExitToLoadingScene()
    {
        Debug.Log("🔄 Switching to Loading Screen...");
        SceneManager.LoadScene("LoadingScene"); // Ensure this scene name is correct
    }
}
