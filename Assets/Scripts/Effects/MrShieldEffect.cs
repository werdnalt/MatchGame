using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MrShieldEffect", menuName = "ScriptableObjects/Effects/MrShieldEffect", order = 4)]

public class MrShieldEffect : Effect
{
    public int numArmor;
    
    public override bool OnObtained(UnitBehaviour unitBehaviour)
    {
        unitBehaviour.armor += numArmor;
        return true;
    }

    public override bool OnHit(UnitBehaviour attackingUnit, UnitBehaviour defendingUnit, ref int damageAmount)
    {
        defendingUnit.armor -= numArmor;
        return true;
    }
}
