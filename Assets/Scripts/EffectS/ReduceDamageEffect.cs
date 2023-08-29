using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReduceDamageEffect", menuName = "ScriptableObjects/Effects/ReduceDamageEffect", order = 5)]
public class ReduceDamageEffect : Effect
{
    public override void OnHit(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        damageAmount = 1;
    }
}
