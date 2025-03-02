using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameTimer: MonoBehaviour
{
    public float timeRemaining = 120f; // 2 minutes set-up begins
    public bool timerIsRunning = false; // At first, thr timer is not running

    public Text timerText; // The timer text is ready to be displayed on the screen
                               // old version: Text

    public PlayerController playerController; // The player needs to be able to move the ball to the final destination within 2 minutes

    private bool playerWon = false;

    // Start the timer when the game starts
    void Start() {
        timerIsRunning = true;
        updateTimeDisplay();
    }
    
    // Update the timer till 0
    void Update() {
        if (timerIsRunning) {
             // As the time is decreasing from 2 minutes, there are 2 specific possibilities required to take into account:
             // (1) does not run out of time
             // (2) run out of time

             if (timeRemaining > 0) {
                
                // The remaining time is greater than 0
                timeRemaining -= Time.deltaTime;

                // The time can be updated
                updateTimeDisplay();
             }
             else {
                // The remaining time is 0
                  timeRemaining = 0;
                  timerIsRunning = false;
                  updateTimeDisplay();
                  loseGame();
             }
        }
    }

    private void updateTimeDisplay() {
        int seconds = Mathf.FloorToInt(timeRemaining);
        // Debug.Log("Time Remaining: " + seconds.ToString()); // Log the remaining time
        timerText.text = "Time Remaining: " + seconds.ToString() + "s";
    }


    public void checkWinCondition() {
        if (timeRemaining > 0) {
            timerIsRunning = false;
            playerWon = true;
            winGame();
        }
    }

    private void winGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void loseGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}