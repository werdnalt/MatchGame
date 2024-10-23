using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThornsEffect", menuName = "ScriptableObjects/Effects/ThornsEffect", order = 4)]
public class ThornsEffect : Effect
{
    public int thornsDamage;
    
    public override bool OnHit(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        if (attackingUnit == attackedUnit) return false;
        
        attackingUnit.TakeDamage(thornsDamage, attackedUnit);
        return true;
    }
}
