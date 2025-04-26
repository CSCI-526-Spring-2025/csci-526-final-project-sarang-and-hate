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
    public Button nextLevelButton;
    private bool playerWon = false;
    public GameObject navPanel; // Displays WASD keys for 10 secs

    // Help button and panel
    public GameObject helpPanel;
    public Button helpButton;
    public Button exitHelpButton; // Exit button for loading scene

    // Instruction Panel Elements
    public GameObject instructionPanel; // Instruction popup
    public TMP_Text instructionText; // TextMeshPro for instructions
    public Button nextInstructionButton; // "Continue" button
    private int instructionIndex = 0;

    //to change the text of the helpPanel 
    public TMP_Text helpPanelText;

    public Button toggleKeysButton;  // The button beneath "Stuck?"
    private bool keysVisible = false;
    public static bool isPlayerStuck = false; // track analytics for deadlock
    public static bool didPlayerRanOutOfTime = false; // track analytics for not completing level within stated time
    public static string currentLevelPlayed = "Level 1";

    // Different Instructions for Each Scene
    private string[] instructions;

    // Instructions for Each Level
    private string[] sampleSceneInstructions = new string[]
    {
        "Follow the arrow to complete the Tutorial!",
        "",
        "Goal: Reach the Blue Tile!",
    };

    private string[] scene2Instructions = new string[]
    {
        "There can be trap tiles, so be careful!",
        "",
        "Capture capsule to gain Special Powers [Go Through the walls]!"
    };

    void Start()
    {
        string sceneName1 = SceneManager.GetActiveScene().name;
        // Skip timer entirely for Tutorial
        if (sceneName1 == "3DTutorialScene")
        {
            if (timerText != null)
                timerText.gameObject.SetActive(false); // hide the timer UI
            return; // skip timer logic entirely
        }
        //Ensures that each level will have 3 resets remaining
         GameSession.helpUsesRemaining = 3; // Reset Help for each level
        //Stop the timer initially
        timerIsRunning = false;

        // Select Instructions Based on the Scene
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "3DTutorialScene")
        {
            instructions = sampleSceneInstructions;
            currentLevelPlayed = "Tutorial";
        }
        else if (sceneName == "Scene2")
        {
            instructions = scene2Instructions;
        }
        else
        {
            instructions = new string[] { "Goal: Reach the Blue Tile!" }; // Fallback
            if (sceneName == "3DScene2")
            {
                currentLevelPlayed = "Level 1";
            }
            else if (sceneName == "3DScene3")
            {
                currentLevelPlayed = "Level 2";
            }
        }

        // Skip Game 
        if (GameSession.skipInstructions)
        {
            instructionPanel?.SetActive(false);
            Time.timeScale = 1;
            timerIsRunning = true;
            GameSession.skipInstructions = false; // Reset for next level
            return;
        }

        //show instructions normally
        if (instructionPanel != null && instructionText != null && nextInstructionButton != null)
        {
            instructionIndex = 0;
            instructionPanel.SetActive(true);
            instructionText.text = instructions[instructionIndex];
            nextInstructionButton.onClick.RemoveAllListeners(); // Prevent double listeners
            nextInstructionButton.onClick.AddListener(ShowNextInstruction);
            Time.timeScale = 0;
        }

        UpdateTimeDisplay();

        // Help button setup
        if (helpButton != null)
        {
            helpButton.onClick.AddListener(OpenHelp);
        }
        if (helpButton != null)
        {
            helpButton.interactable = true; // 🔁 Reset to interactable at level start
        }
        //Add a listener for the nav panel 
        // if (toggleKeysButton != null)
        // {
        //     toggleKeysButton.onClick.AddListener(ToggleNavPanel);
        // }


        if (exitHelpButton != null)
        {
            exitHelpButton.onClick.AddListener(ExitToLoadingScene);
        }

        if (helpPanel != null)
        {
            helpPanel.SetActive(false); // Hide help panel initially
        }

        if (navPanel != null) {
            navPanel.SetActive(false);
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

            // ✅ Only show navPanel in TutorialScene 
            // if (SceneManager.GetActiveScene().name == "3DTutorialScene" && timeRemaining > 110)
            // {
            //     navPanel.SetActive(true);
            // }
            // else
            // {
            //     navPanel.SetActive(false);
            // }
            //Allow player to do it 
            // if (timeRemaining > 110)
            // {
            //     navPanel.SetActive(true);
            // }
            // else
            // {
            //     navPanel.SetActive(false);
            // }
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
        
        // ✅ Only mark tutorial completed if in the actual tutorial scene
        if (SceneManager.GetActiveScene().name == "3DTutorialScene")
        {
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
            Debug.Log("Tutorial marked as completed ✅");
        }

        if (dimBackground != null) dimBackground.SetActive(true);
        if (victoryPanel != null) victoryPanel.SetActive(true);
        if (victoryText != null) victoryText.text = "Victory!";
        if (restartButton != null) restartButton.gameObject.SetActive(true);
        if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(true);
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

        didPlayerRanOutOfTime = true;

        // Send Data to Google Forms and Spreadsheet for Analytics
        sendToGoogle sendGoogle = FindObjectOfType<sendToGoogle>();
        sendGoogle.Send();
    }

    public void RestartGame()
    {
        Debug.Log("RestartGame was triggered.");
        Time.timeScale = 1;
        GameSession.skipInstructions = true; // Skip instructions if user clicks restart 
        if (helpPanel != null){
            helpPanel.SetActive(false);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Help panel methods
    public void OpenHelp()
    {
        isPlayerStuck = true;
        if (GameSession.helpUsesRemaining > 0)
        {
            GameSession.helpUsesRemaining--;

            Debug.Log("Help used. Remaining: " + GameSession.helpUsesRemaining);

            if (helpPanel != null)
            {
                helpPanelText.color = Color.black;
                helpPanel.SetActive(true);
            }

            // Send Data to Google Forms and Spreadsheet for Analytics
            sendToGoogle sendGoogle = FindObjectOfType<sendToGoogle>();
            sendGoogle.Send();
        }
        else
        {
            Debug.Log("No Help uses remaining!");
            helpButton.interactable = false;

            // Show a warning popup
            if (instructionText != null)
            {
                instructionPanel.SetActive(true);
                instructionText.text = "You've used all your Help attempts!";
                nextInstructionButton.gameObject.SetActive(false); // Hide continue if needed
                Time.timeScale = 0;
            }
        }
    }


    public void ExitToLoadingScene()
    {
        Debug.Log("Switching to Menu Screen...");
        Time.timeScale = 1;

        // Optional: Re-enable if user hasn't used all 3
        if (GameSession.helpUsesRemaining > 0)
        {
            helpButton.interactable = true;
        }

        SceneManager.LoadScene("Menu");
    }


    public void PromoteToNextLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("3DScene2"); // Ensure this scene name is correct
    }

    public void PromoteToLevelThree()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("3DScene3"); // Ensure this scene name is correct
    }

    // Show Next Instruction When "Continue" is Clicked
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

    public void ToggleNavPanel()
    {
        keysVisible = !keysVisible;
        if (navPanel != null)
            navPanel.SetActive(keysVisible);

        // if (toggleKeysButton != null)
        // {
        //     TMP_Text btnText = toggleKeysButton.GetComponentInChildren<TMP_Text>();
        //     if (btnText != null)
        //     {
        //         btnText.text = keysVisible ? "Hide Keys" : "Keys";
        //     }
        // }
    }

}

