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
    private CharacterBehaviour _characterBehaviour;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        _characterBehaviour = GetComponent<CharacterBehaviour>();
        EventManager.Instance.onLevelLoad += AddSelector;
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
        Debug.Log("MOVE");
        Vector2 movement = inputValue.Get<Vector2>();
        Debug.Log(movement);

        if (GameManager.Instance.currentScene == GameManager.Scenes.CharacterSelect)
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

    void OnSelect()
    {
        characterSelector.SelectCharacter();
    }

    void OnDeselect()
    {
        characterSelector.DeselectCharacter();
    }
}
