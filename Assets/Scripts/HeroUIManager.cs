using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroUIManager : MonoBehaviour
{
    [SerializeField] private HeroUI heroUI;

    private Dictionary<UnitBehaviour, HeroUI> heroUIDictionary = new Dictionary<UnitBehaviour, HeroUI>();
    
    private void Awake()
    {
        EventPipe.OnHeroAdded += CreateHeroUI;
        EventPipe.OnTreasureAdded += AddTreasure;
        EventPipe.OnTreasureUse += CountdownTreasure;
    }

    private void CreateHeroUI(UnitBehaviour hero)
    {
        var heroUIInstance = Instantiate(heroUI, transform);
        heroUIInstance.Setup(hero);
        heroUIDictionary.Add(hero, heroUIInstance);
    }

    private void AddTreasure(HeroAndTreasure heroAndTreasure)
    {
        if (heroUIDictionary.TryGetValue(heroAndTreasure.hero, out var heroUIInstance))
        {
            heroUIInstance.AddTreasureUI(heroAndTreasure.treasure);
        }
    }

    private void CountdownTreasure(HeroAndTreasure heroAndTreasure)
    {
        if (heroUIDictionary.TryGetValue(heroAndTreasure.hero, out var heroUIInstance))
        {
            heroUIInstance.CountdownTreasure(heroAndTreasure.treasure);
        }
    }
}
