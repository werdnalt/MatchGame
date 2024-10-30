using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoreDamageEffect", menuName = "ScriptableObjects/Effects/StoreDamageEffect", order = 4)]
public class StoreDamageEffect : Effect
{
    public override bool OnHit(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        attackedUnit.DisplayFloatingText("STORING ENERGY!", 1f);
        attackedUnit.bonusAttack += damageAmount;
        return true;
    }
}
