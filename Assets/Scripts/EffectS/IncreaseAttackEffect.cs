using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseAttackEffect", menuName = "ScriptableObjects/Effects/IncreaseAttackEffect", order = 4)]
public class IncreaseAttackEffect : Effect
{
    public int increaseAmount;

    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        attackingUnit.attack += increaseAmount;
        attackingUnit.DisplayFloatingText("ATK UP!", 1);
        return true;
    }
}