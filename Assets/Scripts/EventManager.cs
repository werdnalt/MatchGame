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
}
