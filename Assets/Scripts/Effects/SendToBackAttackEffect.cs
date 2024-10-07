using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SendToBackAttackEffect", menuName = "ScriptableObjects/Effects/SendToBackAttackEffect", order = 4)]
public class SendToBackAttackEffect : Effect
{
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        if (attackedUnit.currentHp - damageAmount > 0)
        {
            BoardManager.Instance.SendUnitBack(attackedUnit);
        }
        return true;
    }
}
