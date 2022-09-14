using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour
{
    public static CharacterSelection Instance;

    public GameObject pressStartText; 
    
    public List<GameObject> characterSelectionObjects;
    
    void OnStart(InputValue inputValue)
    {
        if (GameManager.Instance.playersInGame.Count >= 1)
        {
            SceneManager.LoadSceneAsync("PlayScene");
        }
    }

    private void Update()
    {
        if (GameManager.Instance.playersInGame.Count >= 1)
        {
            pressStartText.SetActive(true);
        }
        else
        {
            pressStartText.SetActive(false);
        }
    }

    private void Start()
    {
        if (Instance == null) Instance = this;
        //GameManager.Instance.currentScene = GameManager.Scenes.CharacterSelect;
    }

    public void ActivateCharacterSelectorUI(int playerIndex)
    {
        CharacterSelector characterSelector = characterSelectionObjects[playerIndex].GetComponent<CharacterSelector>();
        
        // assign selector to player so they can control UI
        GameManager.Instance.GetPlayerByIndex(playerIndex).characterSelector = characterSelector;
        characterSelector.SetTag(playerIndex);
        characterSelector.RefreshUI();
        characterSelector.playerIndex = playerIndex;
    }

    public void DeactivateCharacterSelectorUI(int playerIndex)
    {
        // characterSelectionObjects[playerIndex] = Instantiate(InactiveCharacterSelector);
    }
}
