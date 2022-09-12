using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour
{
    public static CharacterSelection Instance;
    
    public List<GameObject> characterSelectionObjects;
    
    void OnStart(InputValue inputValue)
    {
        SceneManager.LoadSceneAsync("PlayScene");
    }

    private void Start()
    {
        GameManager.Instance.currentScene = GameManager.Scenes.CharacterSelect;

        if (Instance == null) Instance = this;
    }

    public void ActivateCharacterSelectorUI(int playerIndex)
    {
        CharacterSelector characterSelector = characterSelectionObjects[playerIndex].GetComponent<CharacterSelector>();
        
        // assign selector to player so they can control UI
        GameManager.Instance.GetPlayerByIndex(playerIndex).characterSelector = characterSelector;
        characterSelector.SetTag(playerIndex);
        characterSelector.RefreshUI();
    }

    public void DeactivateCharacterSelectorUI(int playerIndex)
    {
        // characterSelectionObjects[playerIndex] = Instantiate(InactiveCharacterSelector);
    }
}
