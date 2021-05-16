using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameObject levelIntermissionUI;
    public int currentLevel;
    public List<Level> levels = new List<Level>();
    
    void Start()
    {
        if (Instance == null) Instance = this;

        currentLevel = 1;

        DontDestroyOnLoad(this.gameObject);

        EventManager.Instance.onNextLevel += NextLevel;
    }

    public void LevelComplete()
    {
        levelIntermissionUI.gameObject.SetActive(true);
        levelIntermissionUI.GetComponent<LevelIntermission>().Setup();

        if (currentLevel < levels.Count) 
        {
            currentLevel++;
        }
    }

    public void NextLevel(int level)
    {
        levelIntermissionUI.gameObject.SetActive(false);
    }
}
