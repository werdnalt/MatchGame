using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HeroAndTreasure
{
    public UnitBehaviour hero;
    public Treasure treasure;

    public HeroAndTreasure(UnitBehaviour hero, Treasure treasure)
    {
        this.hero = hero;
        this.treasure = treasure;
    }
}

public static class EventPipe
{
    public static event Action OnActionTaken;
    public static void TakeAction()
    {
        OnActionTaken?.Invoke();
    }
    
    public static event Action OnPlayerAttack;
    public static void PlayerAttack()
    {
        OnPlayerAttack?.Invoke();
    }

    public static event Action<UnitBehaviour> OnHeroAdded;
    public static void AddHero(UnitBehaviour hero)
    {
        OnHeroAdded?.Invoke(hero);
    }
    
    public static event Action<UnitBehaviour.Stats> OnHeroStatUpdated;
    public static void UpdateHeroStat(UnitBehaviour.Stats stats)
    {
        OnHeroStatUpdated?.Invoke(stats);
    }

    public static event Action<HeroAndTreasure> OnTreasureAdded;

    public static void AddTreasure(HeroAndTreasure treasure)
    {
        OnTreasureAdded?.Invoke(treasure);
    }

    public static event Action<HeroAndTreasure> OnTreasureUse;

    public static void UseTreasure(HeroAndTreasure treasure)
    {
        OnTreasureUse?.Invoke(treasure);
    }
}
