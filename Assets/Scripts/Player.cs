using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public Selector selector;
    
    public CharacterSelector characterSelector;
    public CharacterUI characterUI;
    public CharacterBehaviour characterBehaviour;
    public Character selectedCharacter;
    public int playerIndex;
    
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        characterBehaviour = GetComponent<CharacterBehaviour>();
        EventManager.Instance.onLevelLoad += AddSelector;
        playerIndex = GetComponent<PlayerInput>().playerIndex;
    }

    private void AddSelector()
    {
        selector.Setup();
    }

    void OnStart(InputValue inputValue)
    {
        if (SceneManager.GetActiveScene().name == "ChooseCharacterScene")
        {
            SceneManager.LoadSceneAsync("PlayScene");
        }
    }
    
    void OnNavigate(InputValue inputValue)
    {
        Vector2 movement = inputValue.Get<Vector2>();

        if (GameManager.Instance.currentScene.name == "ChooseCharacterScene")
        {
            // move down
            if (movement.y < 0)
            {
                characterSelector.ScrollDown();
            }
            
            // move up
            if (movement.y > 0)
            {
                characterSelector.ScrollUp();
            }
        }
    }
}
