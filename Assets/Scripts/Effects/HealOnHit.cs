using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealOnHit", menuName = "ScriptableObjects/Effects/HealOnHit", order = 4)]
public class HealOnHit : Effect
{
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        attackingUnit.Heal(damageAmount);
        return true;
    }
}
