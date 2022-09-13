using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    private Dictionary<int, Player> _playersByIndex = new Dictionary<int, Player>();
    public Scene currentScene;
    
    void Start()
    {
        if (Instance == null) Instance = this;
        DontDestroyOnLoad(this.gameObject);
        SceneManager.activeSceneChanged += OnSceneChanged;
        currentScene = SceneManager.GetActiveScene();
    }
    
    void OnPlayerJoined(PlayerInput playerInput)
    {
        _playersByIndex.Add(playerInput.playerIndex, playerInput.GetComponent<Player>());

        if (currentScene.name == "ChooseCharacterScene" )
        {
            CharacterSelection.Instance.ActivateCharacterSelectorUI(playerInput.playerIndex);
        }
    }

    public Player GetPlayerByIndex(int playerIndex)
    {
        return _playersByIndex[playerIndex];
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        currentScene = next;

        if (currentScene.name == "ChooseCharacterScene")
        {
            for (int i = 0; i < 4; i++)
            {
                if (_playersByIndex.ContainsKey(i))
                {
                    _playersByIndex[i].GetComponent<PlayerInput>().SwitchCurrentActionMap("UI");
                }
            }
        }
        
        if (currentScene.name == "PlayScene")
        {
            for (int i = 0; i < 4; i++)
            {
                if (_playersByIndex.ContainsKey(i))
                {
                    _playersByIndex[i].GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
                }
            }
        }
    }
}
