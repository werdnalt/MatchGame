using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterBehaviour : MonoBehaviour
{
    public Character character;

    private Player _player;
    private Selector _selector;

    private int _currentHP;
    private int _currentAttackAmount;
    private int _currentSpecialAbilityAmount;
    private bool _isShielded;
    private bool _isAlive;
    private CharacterUI _characterUI;

    private void Start()
    {
        _player = GetComponent<Player>();
        _selector = _player.selector;
    }
    
    private void Setup()
    {
        _currentHP = character.hp;
        _currentAttackAmount = 0;
        _isShielded = false;
        _isAlive = true;
        _selector = GetComponent<Selector>();
        _characterUI.RefreshUI();
    }

    void OnSpecialAbility(InputValue inputValue)
    {
        BoardManager.Coordinates[] selectedCoords = _selector.GetSelectedBlocks();
        BoardManager.Instance.ReplaceBlocks(selectedCoords[0], selectedCoords[1], character.specialBlockPrefab);
    }

    public void LevelStart()
    {
        EventManager.Instance.onLevelLoad += Setup;
    }
    
    public void ReceiveDamage(int damage)
    {
        if (_currentHP - damage <= 0)
        {
            _currentHP = 0;
            _isAlive = false;
        }

        else
        {
            _currentHP -= damage;
        }
        _characterUI.RefreshUI();
    }

    public void GainSpecialAbilityCharge(int comboAmount)
    {
        if (_currentSpecialAbilityAmount + comboAmount >= character.specialAbilityRequirement)
        {
            _currentSpecialAbilityAmount = character.specialAbilityRequirement;
        }
        else
        {
            _currentSpecialAbilityAmount += comboAmount;
        }
        _characterUI.RefreshUI();
    }

    public void SetCharacterUI(CharacterUI characterUI)
    {
        _characterUI = characterUI;
    }
}
