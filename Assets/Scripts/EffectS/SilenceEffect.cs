using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SilenceEffect", menuName = "ScriptableObjects/Effects/SilenceEffect", order = 4)]
public class SilenceEffect : Effect
{
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        attackedUnit.SilenceEffects();
        return true;
    }
}
