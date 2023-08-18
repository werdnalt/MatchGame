using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CharacterSelector : MonoBehaviour
{
    public List<Character> allCharacters;
    public Character currentCharacter;
    public int playerIndex; 
    
    [SerializeField] private Image characterImage;
    [SerializeField] private Image specialBlock;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI characterDescription;
    [SerializeField] private Image _playerTag;
    private int _currentCharacterIndex;

    private bool _lockedIn;
    private Coroutine _flash;

    private void Start()
    {
        _currentCharacterIndex = 0;
        currentCharacter = allCharacters[_currentCharacterIndex];
        _lockedIn = false;
        characterImage.material = new Material(characterImage.material);
    }
    

    public void ScrollUp()
    {
        if (!_lockedIn)
        {
            if (_currentCharacterIndex < allCharacters.Count - 1)
            {
                _currentCharacterIndex++;
            }
            else
            {
                _currentCharacterIndex = 0;
            }
            currentCharacter = allCharacters[_currentCharacterIndex];
            Debug.Log("Character index: " + _currentCharacterIndex);
            RefreshUI();
        }
    }

    public void ScrollDown()
    {
        if (!_lockedIn)
        {
            if (_currentCharacterIndex > 0)
            {
                _currentCharacterIndex--;
            }
            else
            {
                _currentCharacterIndex = allCharacters.Count - 1;
            }

            Debug.Log("Character index: " + _currentCharacterIndex);
            currentCharacter = allCharacters[_currentCharacterIndex];
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        specialBlock.enabled = true;
        characterImage.sprite = currentCharacter.characterSprite;
        specialBlock.sprite = currentCharacter.specialBlockSprite;
        characterName.text = currentCharacter.id;
        characterDescription.text = currentCharacter.specialBlockDescription;
    }


    private IEnumerator GreyscaleFlash()
    {
        // disable greyscale shader
        Material mat = characterImage.material;
        float duration = .1f;
        int numFlashes = 3;

        for (int i = 0; i < numFlashes; i++)
        {
            mat.SetFloat("_GreyscaleBlend", 0);
            yield return new WaitForSeconds(duration);
            mat.SetFloat("_GreyscaleBlend", 1);
            yield return new WaitForSeconds(duration);
        }
        mat.SetFloat("_GreyscaleBlend", 0);
    }

    public void DeselectCharacter()
    {
        if (_flash != null) StopCoroutine(_flash);
        characterImage.material.SetFloat("_GreyscaleBlend", 1);
        _lockedIn = false;
        //GameManager.Instance.GetPlayerByIndex(playerIndex).selectedCharacter = null;
    }

    public void SetTag(int index)
    {
        _playerTag.color = new Color(_playerTag.color.r, _playerTag.color.g, _playerTag.color.b, 255);
        _playerTag.sprite = Resources.Load<Sprite>("tags/" + index);
    }
}
