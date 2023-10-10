using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseAttackEffect", menuName = "ScriptableObjects/Effects/IncreaseAttackEffect", order = 4)]
public class IncreaseAttackEffect : Effect
{
    public int increaseAmount;

    public override void OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit)
    {
        attackingUnit.attack += increaseAmount;
        
        attackingUnit.ShowAttack();
        attackingUnit.PlayIncreaseHealthParticles();
        attackingUnit.DisplayFloatingText("ATK UP!", 1);
    }
}
