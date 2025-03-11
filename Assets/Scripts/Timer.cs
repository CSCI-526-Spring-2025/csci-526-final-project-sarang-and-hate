using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

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

    // ✅ Instruction Panel Elements
    public GameObject instructionPanel; // Instruction popup
    public TMP_Text instructionText; // TextMeshPro for instructions
    public Button nextInstructionButton; // "Continue" button
    private int instructionIndex = 0;

    // ✅ Different Instructions for Each Scene
    private string[] instructions;

    // ✅ Instructions for Each Level
    private string[] sampleSceneInstructions = new string[]
    {
        "Welcome to Level 1!",
        "",
        "Goal: Reach the Destination (Blue Tile).",
        "",
        "The walls rotate, so find a way to reach the destination before time runs out!"
    };

    private string[] scene2Instructions = new string[]
    {
        "Welcome to Level 2!",
        "",
        "There can be trap tiles, so be careful!",
        "",
        "Capture capsule to gain Special Powers [Go Through the walls]!"
    };

    void Start()
    {
        // ✅ Stop the timer initially
        timerIsRunning = false;

        // ✅ Select Instructions Based on the Scene
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "SampleScene")
        {
            instructions = sampleSceneInstructions;
        }
        else if (sceneName == "Scene2")
        {
            instructions = scene2Instructions;
        }
        else
        {
            instructions = new string[] { "Default Instructions: Good luck!" }; // Fallback
        }

        if (instructionPanel != null && instructionText != null && nextInstructionButton != null)
        {
            instructionPanel.SetActive(true); // Show the instruction panel
            instructionText.text = instructions[instructionIndex]; // Display first instruction
            nextInstructionButton.onClick.AddListener(ShowNextInstruction);
            Time.timeScale = 0; // Pause game while instructions are displayed
        }
        else
        {
            Debug.LogError("Instruction panel, text, or button is not assigned in the Inspector!");
        }

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
        if (dimBackground != null) dimBackground.SetActive(true);
        if (victoryPanel != null) victoryPanel.SetActive(true);
        if (victoryText != null) victoryText.text = "Victory!";
        if (restartButton != null) restartButton.gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    private void ShowRestartPanel()
    {
        Debug.Log("⏳ Time's Up! Player failed to complete the maze in time.");
        if (dimBackground != null) dimBackground.SetActive(true);
        if (restartPanel != null) restartPanel.SetActive(true);
        if (victoryText != null) victoryText.text = "Time Over!";
        if (restartButton != null) restartButton.gameObject.SetActive(true);
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

    public void PromoteToNextLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Scene2"); // Ensure this scene name is correct
    }

    // ✅ Show Next Instruction When "Continue" is Clicked
    public void ShowNextInstruction()
    {
        instructionIndex++; // Increment to next instruction
        
        if (instructionIndex < instructions.Length) 
        {
            // Show the next instruction
            instructionText.text = instructions[instructionIndex];
            instructionText.ForceMeshUpdate(); // Ensure TMP refreshes layout
            Debug.Log("Showing Instruction: " + instructionIndex + " - " + instructionText.text);
        }
        else
        {
            // We've gone through all instructions
            instructionPanel.SetActive(false);
            Time.timeScale = 1; // Resume game
            timerIsRunning = true; // Start the timer
            Debug.Log("All instructions completed. Game starts now.");
        }
    }
}

