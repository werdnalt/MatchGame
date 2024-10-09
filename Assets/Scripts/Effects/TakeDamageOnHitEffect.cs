using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TakeDamageOnHitEffect", menuName = "ScriptableObjects/Effects/TakeDamageOnHitEffect", order = 5)]

public class TakeDamageOnHitEffect : Effect
{
    public int dmg;
    
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        attackingUnit.TakeDamage(dmg, attackingUnit);
        return true;
    }
}
