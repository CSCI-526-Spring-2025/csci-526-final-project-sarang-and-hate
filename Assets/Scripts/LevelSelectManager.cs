using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.Collections.Generic;


public class LevelSelectManager : MonoBehaviour
{
    public void LoadLevel1()
    {
        Time.timeScale = 1; // Just in case coming from a paused scene
        SceneManager.LoadScene("3DTutorialScene"); // Update if scene name is different
    }

    public void LoadLevel2()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("3DScene2");
    }

    public void LoadLevel3()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("3DScene3");
    }
}
