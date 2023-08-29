using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReflectDamageEffect", menuName = "ScriptableObjects/Effects/ReflectDamageEffect", order = 3)]
public class ReflectDamageEffect : Effect
{
    public override void OnHit(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        attackingUnit.TakeDamage(attackingUnit.attack, attackedUnit);
    }
}
