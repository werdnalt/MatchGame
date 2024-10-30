using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UseStoredDamageEffect", menuName = "ScriptableObjects/Effects/UseStoredDamageEffect", order = 2)]
public class UseStoredDamageEffect : Effect  
{
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        if (attackingUnit.bonusAttack > 0)
        {
            attackingUnit.DisplayFloatingText("RELEASE ENERGY!", 1f);
        }
        attackingUnit.bonusAttack = 0;
        return true;
    }
}
