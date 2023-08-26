using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect : ScriptableObject
{
    protected UnitBehaviour _unit;
    public string effectDescription;

    public void SetUnit(UnitBehaviour unit)
    {
        _unit = unit;
    }
    
    // This function gets called when a block dies.
    public virtual void OnDeath() { }
    
    // This function gets called when a block moves.
    public virtual void OnMove() { }
    
    // This function gets called when a block swaps.
    public virtual void OnSwap() { }
    
    // This function gets called when a block falls.
    public virtual void OnFall() { }
    
    public virtual void OnAttack() { }
}
