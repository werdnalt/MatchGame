using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MyDebugger : MonoBehaviour
{
    public static MyDebugger Instance;

    [SerializeField] private TextMeshProUGUI debugText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ShowDebugText(string text)
    {
        debugText.text = text;
    }
}
