using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
    public Button tutorialButton;
    public Button level2Button;
    public Button level3Button;
    public Button level4Button; // 🆕 ADD THIS

    public TextMeshProUGUI level2Text;
    public TextMeshProUGUI level3Text;
    public TextMeshProUGUI level4Text; // 🆕 ADD THIS if you want "Locked" text too

    void Start()
    {
        // Check if the tutorial has been completed (stored in PlayerPrefs)
        bool tutorialDone = PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;

        // Enable or disable Level 2, 3, and 4 buttons based on tutorial status
        level2Button.interactable = tutorialDone;
        level3Button.interactable = tutorialDone;
        level4Button.interactable = tutorialDone; // 🆕 ADD THIS

        // Gray out buttons if locked
        if (!tutorialDone)
        {
            level2Button.image.color = Color.gray;
            level3Button.image.color = Color.gray;
            level4Button.image.color = Color.gray; // 🆕 ADD THIS

            // Optional: show "Locked" on buttons
            // if (level2Text != null) level2Text.text = "Locked";
            // if (level3Text != null) level3Text.text = "Locked";
            // if (level4Text != null) level4Text.text = "Locked"; // 🆕 ADD THIS
        }
    }

    public void LoadTutorial()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("3DTutorialScene"); // Update if your tutorial scene name differs
    }

    public void LoadLevel2()
    {
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("3DScene2");
        }
    }

    public void LoadLevel3()
    {
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("3DScene3");
        }
    }

    public void LoadLevel4()
    {
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("3DScene4");
        }
    }
}
