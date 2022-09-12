using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public event Action<int> onNextLevel;
    public void NextLevel(int level)
    {
        onNextLevel?.Invoke(level);
    }

    public event Action onGameComplete;
    public void GameComplete()
    {
        onGameComplete?.Invoke();
    }

    public event Action onMainMenu;
    public void MainMenu()
    {
        onMainMenu?.Invoke();
    }
    
    public event Action onGameStart;
    public void StartGame()
    {
        onGameStart?.Invoke();
    }
    
    public event Action onLevelLoad;
    public void LevelLoaded()
    {
        onLevelLoad.Invoke();
    }
}
