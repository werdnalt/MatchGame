using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FallIncreaseAttackEffect", menuName = "ScriptableObjects/Effects/FallIncreaseAttackEffect", order = 2)]
public class FallIncreaseAttackEffect : Effect
{
    public int minDistanceFallen;
    public int attackIncreaseAmount;
    
    public override bool OnFall(UnitBehaviour fallenUnit, int distanceFallen)
    {
        if (distanceFallen >= minDistanceFallen)
        {
            fallenUnit.IncreaseAttack(attackIncreaseAmount);
            return true;
        }

        return false;
    }
}
