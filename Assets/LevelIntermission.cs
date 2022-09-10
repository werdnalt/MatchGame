using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelIntermission : MonoBehaviour
{
    public TextMeshProUGUI topButtonText;
    public TextMeshProUGUI playerScore;

    public void LoadNextLevel()
    {
        // if there is a next level, load the next level
        if (GameManager.Instance.currentLevel == GameManager.Instance.levels.Count)
        {
            EventManager.Instance.NextLevel(GameManager.Instance.currentLevel);
        } else {
            EventManager.Instance.GameComplete();
            SceneManager.LoadScene("GameCompleteScene");
        }
    }

    public void LoadMainMenu()
    {
        EventManager.Instance.MainMenu();
        SceneManager.LoadScene("MainMenuScene");
    }

    public void Setup()
    {
        if (GameManager.Instance.currentLevel >= GameManager.Instance.levels.Count)
        {
            topButtonText.text = "GAME OVER";
        } else {
            topButtonText.text = "NEXT LEVEL";
        }
    }
}
