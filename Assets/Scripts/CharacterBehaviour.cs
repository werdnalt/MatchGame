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
