using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameObject levelIntermissionUI;
    public int currentLevel;
    public List<Level> levels = new List<Level>();

    private Dictionary<int, Player> _playersByIndex = new Dictionary<int, Player>();

    public Scenes currentScene;

    public enum Scenes
    {
        CharacterSelect,
        Play,
        MainMenu,
    }
    
    void Start()
    {
        if (Instance == null) Instance = this;

        currentLevel = 1;

        DontDestroyOnLoad(this.gameObject);

        // EventManager.Instance.onNextLevel += NextLevel;
        // EventManager.Instance.onMainMenu += HideIntermissionUI;
        // EventManager.Instance.onGameComplete += HideIntermissionUI;
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

    public void HideIntermissionUI()
    {
        levelIntermissionUI.gameObject.SetActive(false);
    }

    void OnPlayerJoined(PlayerInput playerInput)
    {
        _playersByIndex.Add(playerInput.playerIndex, playerInput.GetComponent<Player>());

        if (currentScene == Scenes.CharacterSelect)
        {
            CharacterSelection.Instance.ActivateCharacterSelectorUI(playerInput.playerIndex);
        }
    }

    public Player GetPlayerByIndex(int playerIndex)
    {
        return _playersByIndex[playerIndex];
    }
}
