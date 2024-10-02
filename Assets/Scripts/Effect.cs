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
    
    public void SetUnitBehaviour(UnitBehaviour unitBehaviour)
    {
        _unitBehaviour = unitBehaviour;
    }
    
    // This function gets called when a block dies.
    public virtual void OnKill(UnitBehaviour killedBy, UnitBehaviour killed) { }
    
    // This function gets called when a block dies.
    public virtual bool OnDeath(UnitBehaviour killedBy, UnitBehaviour killed)
    {
        return false;}
    
    // This function gets called when a block swaps.
    public virtual bool OnSwap(Coordinates swappedUnitCoordinates, Coordinates swappedByUnitCoordinates)
    {
        return false;}
    
    // This function gets called when a block falls.
    public virtual void OnFall() { }

    public virtual bool OnHit(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    { return false; }

    public virtual bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        return false; }

    public virtual void OnObtained(UnitBehaviour unitObtained)
    {
        
    }
}
