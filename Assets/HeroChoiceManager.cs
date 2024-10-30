using System;
using System.Collections;
using System.Collections.Generic;
using AllIn1SpringsToolkit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroChoiceManager : MonoBehaviour
{
    public int numHeroes = 4;

    [SerializeField] private Button _startGameButton;
    [SerializeField] private PopupTooltip _popupTooltip;
    [SerializeField] private List<GameObject> _heroChoicePlaceholders;
    
    private List<Unit> _chosenHeroes = new List<Unit>();

    public bool IsRoomForHero()
    {
        return _chosenHeroes.Count < numHeroes;
    }
    
    public bool TryToAddHero(Unit hero, TransformSpringComponent heroChoiceObjectSpring)
    {
        if (_chosenHeroes.Count < numHeroes)
        {
            _chosenHeroes.Add(hero);
            var numHeroesChosen = _chosenHeroes.Count;
            heroChoiceObjectSpring.targetTransform = _heroChoicePlaceholders[numHeroesChosen - 1].transform;
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
}
