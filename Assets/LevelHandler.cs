using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelHandler : MonoBehaviour
{
    public static LevelHandler Instance;

    public int currentLevel;
    public int maxLevel;

    void Start()
    {
        if (Instance == null) Instance = this;
        currentLevel = 1;

        // set up current enemy
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene("PlayScene");
    }

    public void LoadSettingsMenu()
    {
        SceneManager.LoadScene("SettingsScene");
    }
}
