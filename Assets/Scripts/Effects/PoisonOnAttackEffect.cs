using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoisonOnAttackEffect", menuName = "ScriptableObjects/Effects/PoisonOnAttackEffect", order = 4)]
public class PoisonOnAttackEffect : Effect
{
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        var chain = BoardManager.Instance.GetChainedUnits(attackedUnit);
        foreach (var unit in chain)
        {
            unit.AddStatus(new UnitBehaviour.Status(UnitBehaviour.StatusEffect.Poisoned, attackingUnit, 2));
            unit.DisplayFloatingText("POISONED", 1);
        }
        return true;
    }
}
