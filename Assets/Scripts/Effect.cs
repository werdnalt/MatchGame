using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect : ScriptableObject
{
    protected UnitBehaviour _unitBehaviour;
    public string effectDescription;
    public Sprite effectSprite;
    public int numUses;
    public Treasure fromTreasure = null;
    public int actionCost = 1;
    public bool hasChainEffect;
    
    public virtual void SetUnitBehaviour(UnitBehaviour unitBehaviour)
    {
        _unitBehaviour = unitBehaviour;
    }
    
    // This function gets called when a block dies.
    public virtual bool OnKill(UnitBehaviour killedBy, UnitBehaviour killed)
    {
        return false;
    }
    
    // This function gets called when a block dies.
    public virtual bool OnDeath(UnitBehaviour killedBy, UnitBehaviour killed)
    {
        return false;}
    
    // This function gets called when a block swaps.
    public virtual bool OnSwap(Coordinates swappedUnitCoordinates, Coordinates swappedByUnitCoordinates)
    {
        return false;}
    
    // This function gets called when a block falls.
    public virtual bool OnFall(UnitBehaviour fallenUnit, int distanceFallen)
    {
        return false;
        
    }

    public virtual bool OnHit(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    { return false; }

    public virtual bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        return false; }

    public virtual bool OnObtained(UnitBehaviour unitObtained)
    {
        return false;
    }

    public virtual void RemoveEffect(UnitBehaviour unitBehaviour)
    {
        
    }
    
    public virtual bool OnChain(UnitBehaviour chainedUnit)
    {
        return false;
    }
}
