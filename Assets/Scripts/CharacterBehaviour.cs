using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterBehaviour : MonoBehaviour
{
    public Character character;

    private Player _player;
    private Selector _selector;
    
    private int _currentHP;
    
    private int _currentBombPoints;
    
    private int _currentSpecialAbilityAmount;
    private int _numSpecialAbilityRequired;
    
    private int _currentPoints;
    
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
        EventManager.Instance.onLevelLoad += Setup;
        _player = GetComponent<Player>();
        _selector = _player.selector;
        
        _bombPointsRequirements.Add(3);
        _bombPointsRequirements.Add(6);
        _bombPointsRequirements.Add(10);
        _bombPointsRequirements.Add(100000);
    }
    
    private void Setup()
    {
        Debug.Log("SETUP");
        _currentHP = 3;
        _currentBombPoints = 0;
        _isShielded = false;
        _isAlive = true;
        _selector = GetComponent<Selector>();
    }

    void OnSpecialAbility(InputValue inputValue)
    {
        if (_currentSpecialAbilityAmount >= _numSpecialAbilityRequired)
        {
            BoardManager.Coordinates[] selectedCoords = _selector.GetSelectedBlocks();
            BoardManager.Instance.ReplaceBlocks(selectedCoords[0], selectedCoords[1], character.specialBlockPrefab);
            BoardManager.Instance.ShowDeployAnimation(selectedCoords[0]);
            BoardManager.Instance.ShowDeployAnimation(selectedCoords[1]);

            _currentSpecialAbilityAmount = 0;
            _characterUI.UpdateSpecialAbilityBar(0, _numSpecialAbilityRequired);
        }
    }

    void OnAttack(InputValue inputValue)
    {
        Bomb.BombType type = Bomb.BombType.Landmine;
        if (_currentBombPoints >= 50)
        {
            if (_currentBombPoints >= 50 && _currentBombPoints <= 150)
            {
                type = Bomb.BombType.Landmine;
            }
        
            if (_currentBombPoints >150 && _currentBombPoints <= 300)
            {
                type = Bomb.BombType.Round;
            }
        
            if (_currentBombPoints > 300)
            {
                type = Bomb.BombType.Atomic;
            }
            
            BoardManager.Coordinates[] selectedCoords = _selector.GetSelectedBlocks();
            GameObject bomb1 = Instantiate(BoardManager.Instance.bombPrefab);
            bomb1.GetComponent<Bomb>().Setup(type, GetComponent<PlayerInput>().playerIndex,selectedCoords[0], 7);
            BoardManager.Instance.ReplaceBlock(selectedCoords[0], bomb1);
            BoardManager.Instance.ShowDeployAnimation(selectedCoords[0]);
        
            GameObject bomb2 = Instantiate(BoardManager.Instance.bombPrefab);
            bomb2.GetComponent<Bomb>().Setup(type, GetComponent<PlayerInput>().playerIndex,selectedCoords[1], 7);
            BoardManager.Instance.ReplaceBlock(selectedCoords[1], bomb2);
            BoardManager.Instance.ShowDeployAnimation(selectedCoords[1]);

            _currentBombPoints = 0;
            _characterUI.UpdateBombUI(0);
        }
    }

    public void LevelStart()
    {
        
    }
    
    public void ReceiveDamage(int damage)
    {
        if (_isAlive)
        {
            Debug.Log("DAMAGE AMOUNT: " + damage);
            if (_currentHP - damage <= 0)
            {
                Die();
                _currentHP = 0;
            }

            else
            {
                _currentHP -= damage;
            }
            _characterUI.DamageFlash();
            _characterUI.RefreshUI();
        }
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
        _characterUI.UpdateSpecialAbilityBar(_currentSpecialAbilityAmount, _numSpecialAbilityRequired);
        //_characterUI.RefreshUI();
    }

    public void SetCharacterUI(CharacterUI characterUI)
    {
        _characterUI = characterUI;
        characterUI.Setup(character);
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
        
        _characterUI.UpdateBombUI(_currentBombPoints);
    }

    public void GainPoints(int amount)
    {
        _currentPoints += amount;
    }

    private void UpdateBombUI()
    {
        
    }

    private void Die()
    {
        _selector.gameObject.SetActive(false);
        _characterUI.SetDead();
        GameManager.Instance.PlayerDied(_player);
        _isAlive = false;
    }

    public void OnReset(InputValue inputValue)
    {
        SceneManager.LoadSceneAsync("ChooseCharacterScene");
    }
}
