using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterBehaviour : MonoBehaviour
{
    public Character character;

    private Player _player;
    private Selector _selector;

    private int _currentPoints;
    private int _currentHP;
    private int _currentBombPoints;
    private int _currentSpecialAbilityAmount;
    private bool _isShielded;
    private bool _isAlive;
    private CharacterUI _characterUI;

    public List<GameObject> bombPrefabs;
    private List<int> _bombPointsRequirements = new List<int>();
    private int _currentBombLevel;

    public enum CharacterStats
    {
        Attack,
        Special,
        Damage,
        Shield
    }

    private void Start()
    {
        _player = GetComponent<Player>();
        _selector = _player.selector;
        
        _bombPointsRequirements.Add(3);
        _bombPointsRequirements.Add(6);
        _bombPointsRequirements.Add(10);
        _bombPointsRequirements.Add(100000);
    }
    
    private void Setup()
    {
        _currentHP = character.hp;
        _currentBombPoints = 0;
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

    void OnAttack(InputValue inputValue)
    {
        BoardManager.Coordinates[] selectedCoords = _selector.GetSelectedBlocks();
        GameObject bomb1 = Instantiate(BoardManager.Instance.bombPrefab);
        bomb1.GetComponent<Bomb>().Setup(Bomb.BombType.Atomic, GetComponent<PlayerInput>().playerIndex,selectedCoords[0], 7);
        BoardManager.Instance.ReplaceBlock(selectedCoords[0], bomb1);
        
        GameObject bomb2 = Instantiate(BoardManager.Instance.bombPrefab);
        bomb2.GetComponent<Bomb>().Setup(Bomb.BombType.Atomic, GetComponent<PlayerInput>().playerIndex,selectedCoords[1], 7);
        BoardManager.Instance.ReplaceBlock(selectedCoords[1], bomb2);
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
        //_characterUI.RefreshUI();
    }

    public void SetCharacterUI(CharacterUI characterUI)
    {
        _characterUI = characterUI;
    }

    public void SetCharacter(Character character)
    {
        this.character = character;
    }

    public void GainAttack(int attackPoints)
    {
        _currentBombPoints += attackPoints;
        if (_currentBombPoints > _bombPointsRequirements[_currentBombLevel])
        {
            _currentBombLevel++;
        }
    }

    private void UpdateBombUI()
    {
        
    }
}
