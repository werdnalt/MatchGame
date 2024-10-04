using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroChoiceManager : MonoBehaviour
{
    public int numHeroes = 4;

    [SerializeField] private Button _startGameButton;
    [SerializeField] private PopupTooltip _popupTooltip;
    
    private List<Unit> _chosenHeroes = new List<Unit>();

    public bool IsRoomForHero()
    {
        return _chosenHeroes.Count < numHeroes;
    }
    
    public bool TryToAddHero(Unit hero)
    {
        if (_chosenHeroes.Count < numHeroes)
        {
            _chosenHeroes.Add(hero);
            UpdateButtonState();
            return true;
        }
        
        return false;
    }

    private void UpdateButtonState()
    {
        var heroesLeft = numHeroes - _chosenHeroes.Count;

        if (heroesLeft <= 0)
        {
            _startGameButton.interactable = true;
            _startGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Game!";
        }
        
        if (heroesLeft > 1)
        {
            _startGameButton.interactable = false;
            _startGameButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Choose {heroesLeft} More Heroes";
        }
        else
        {
            _startGameButton.interactable = false;
            _startGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Choose 1 More Hero";
        }
    }

    public void RemoveHero(Unit hero)
    {
        if (_chosenHeroes.Contains(hero)) _chosenHeroes.Remove(hero);
        UpdateButtonState();
    }

    public void ShowTooltip(Unit unit)
    {
        _popupTooltip.ShowUnitPanel(unit);
    }

    public void HideTooltip()
    {
        _popupTooltip.Hide();
    }
}
