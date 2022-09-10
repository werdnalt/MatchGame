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
}
