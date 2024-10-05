using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseRangeEffect", menuName = "ScriptableObjects/Effects/IncreaseRangeEffect", order = 4)]
public class IncreaseRangeEffect : Effect
{
    public int extraRange;
    
    public override bool OnObtained(UnitBehaviour unit)
    {
        unit.attackRange += extraRange;
        return true;
    }

    public override void RemoveEffect(UnitBehaviour unit)
    {
        unit.attackRange -= extraRange;
    }
}
