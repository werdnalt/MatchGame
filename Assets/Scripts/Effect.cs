using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect : ScriptableObject
{
    protected UnitBehaviour _unitBehaviour;
    public string effectDescription;
    public Sprite effectSprite;

    public void SetUnitBehaviour(UnitBehaviour unitBehaviour)
    {
        _unitBehaviour = unitBehaviour;
    }
    
    // This function gets called when a block dies.
    public virtual void OnKill(UnitBehaviour killedBy, UnitBehaviour killed) { }
    
    // This function gets called when a block dies.
    public virtual void OnDeath(UnitBehaviour killedBy, UnitBehaviour killed) { }
    
    // This function gets called when a block swaps.
    public virtual void OnSwap(UnitBehaviour swappedUnit, UnitBehaviour swappedWith) { }
    
    // This function gets called when a block falls.
    public virtual void OnFall() { }
    
    public virtual void OnHit(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount) { }
    
    public virtual void OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit) { }
}
