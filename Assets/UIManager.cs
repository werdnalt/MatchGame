using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public List<GameObject> characterUIsGameObjects;
    private List<CharacterUI> _characterUIs = new List<CharacterUI>();

    private void Awake()
    {
        if (Instance == null) Instance = this;

        EventManager.Instance.onBoardReady += ShowUI;
    }

    public void ShowUI()
    {
        for (int i = 0; i < GameManager.Instance.playersInGame.Count; i++)
        {
            characterUIsGameObjects[i].SetActive(true);
            GameManager.Instance.playersInGame[i].AssignPlayerUI(characterUIsGameObjects[i].GetComponent<CharacterUI>());
        }
        
        foreach (var UIGO in characterUIsGameObjects)
        {
            _characterUIs.Add(UIGO.GetComponent<CharacterUI>());
        }
    }

    public void UpdateUI(int playerIndex)
    {
        
    }
}
