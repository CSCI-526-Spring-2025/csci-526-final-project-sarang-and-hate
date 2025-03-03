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

    void Start()
    {
        timerIsRunning = true;
        UpdateTimeDisplay();
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
}