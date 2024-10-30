using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackOnChain", menuName = "ScriptableObjects/Effects/AttackOnChain", order = 2)]
public class AttackOnChain : Effect
{
    public int attackIncreaseAmount;
    
    private void Awake()
    {
        hasChainEffect = true;
    }

    public override bool OnChain(UnitBehaviour chainedUnit)
    {
        var chainedUnits = BoardManager.Instance.GetChainedUnits(chainedUnit);
        foreach (var unit in chainedUnits)
        {
            unit.IncreaseAttack(attackIncreaseAmount);
        }
        
        return true;
    }

    public override void RemoveEffect(UnitBehaviour chainedUnit)
    {
        var chainedUnits = BoardManager.Instance.GetChainedUnits(chainedUnit);
        foreach (var unit in chainedUnits)
        {
            unit.IncreaseAttack(attackIncreaseAmount);
        }
    }
}
