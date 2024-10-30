using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResetAttackTimerEffect", menuName = "ScriptableObjects/Effects/ResetAttackTimerEffect", order = 2)]
public class ResetAttackTimerEffect : Effect
{
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        if (attackedUnit is EnemyUnitBehaviour)
        {
            var enemyUnit = attackedUnit as EnemyUnitBehaviour;
            enemyUnit.ResetAttackTimer();
            return true;
        }

        return false;
    }
}
