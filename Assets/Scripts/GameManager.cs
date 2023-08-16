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
    public List<Player> playersInGame = new List<Player>();
    private List<Player> _alivePlayers = new List<Player>();
    public Scene currentScene;
    public bool canMove;
    public List<GameObject> readyObjects;
    public GameObject gameOver;
    
    void Start()
    {
        if (Instance == null) Instance = this;
        DontDestroyOnLoad(this.gameObject);
        SceneManager.activeSceneChanged += OnSceneChanged;
        currentScene = SceneManager.GetActiveScene();
        canMove = true;
    }
    
    public void OnPlayerJoined(PlayerInput playerInput)
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
            AudioManager.Instance.PlayAndLoop("selectchar");
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
            _alivePlayers = playersInGame;
            AudioManager.Instance.PlayAndLoop("game");
            for (int i = 0; i < 4; i++)
            {
                if (_playersByIndex.ContainsKey(i))
                {
                    _playersByIndex[i].GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
                }
            }

            canMove = true;
        }
    }
    
    public void LockPlayerIntoGame(Player player)
    {
        if (!playersInGame.Contains(player))
        {
            playersInGame.Add(player);
        }
    }

    public void PlayerDied(Player player)
    {
        _alivePlayers.Remove(player);
        if (_alivePlayers.Count <= 1) Debug.Log("GAME OVER, PLAYER " + player.GetComponent<PlayerInput>().playerIndex + 1 +" WINS!");
        gameOver.SetActive(true);
    }

    

    private IEnumerator StartGame()
    {
        int currentlyDisplaying = 0;
        yield return new WaitForSeconds(2);

        for (int i = 0; i <= readyObjects.Count; i++)
        {
            if (currentlyDisplaying < readyObjects.Count) readyObjects[currentlyDisplaying].SetActive(true);
            if (currentlyDisplaying - 1 >= 0) readyObjects[currentlyDisplaying - 1].SetActive(false);
            currentlyDisplaying++;
            yield return new WaitForSeconds(1);
        }

        canMove = true;
    }
}
