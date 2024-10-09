using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoreDamageEffect", menuName = "ScriptableObjects/Effects/StoreDamageEffect", order = 4)]
public class StoreDamageEffect : Effect
{
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        if (attackingUnit.bonusAttack > 0)
        {
            attackingUnit.DisplayFloatingText("RELEASE ENERGY!", 1f);
        }
        damageAmount += attackingUnit.bonusAttack;
        attackingUnit.bonusAttack = 0;
        return true;
    }
    
    public override bool OnHit(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        attackedUnit.DisplayFloatingText("STORING ENERGY!", 1f);
        attackedUnit.bonusAttack += damageAmount;
        return true;
    }
}
