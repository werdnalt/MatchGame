using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealOnAttackEffect", menuName = "ScriptableObjects/Effects/HealOnAttackEffect", order = 4)]
public class HealOnAttackEffect : Effect
{
    public int healAmount;
    
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        attackingUnit.Heal(healAmount);
        return true;
    }
}
