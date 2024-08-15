using System;
using UnityEngine;

public class GamePlayDirector : MonoBehaviour
{
    public static GamePlayDirector Instance;
    public bool PlayerActionAllowed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
}