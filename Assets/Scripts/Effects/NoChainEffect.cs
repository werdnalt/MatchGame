using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NoChainEffect", menuName = "ScriptableObjects/Effects/NoChainEffect", order = 5)]

public class NoChainEffect : Effect
{
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        attackingUnit.cantChain = true;
        attackingUnit.canChainAttack = false;
        return true;
    }
}
