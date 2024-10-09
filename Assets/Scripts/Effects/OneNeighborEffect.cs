using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OneNeighborEffect", menuName = "ScriptableObjects/Effects/OneNeighborEffect", order = 4)]
public class OneNeighborEffect : Effect
{
    public int extraDamageAmount;
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        var leftNeighbor =
            BoardManager.Instance.GetSingleNeighborCoordinates(attackingUnit.currentCoordinates,
                BoardManager.Direction.Left);
        var rightNeighbor = BoardManager.Instance.GetSingleNeighborCoordinates(attackingUnit.currentCoordinates,
            BoardManager.Direction.Right);

        if (!BoardManager.Instance.GetUnitBehaviour(leftNeighbor) ||
            !BoardManager.Instance.GetUnitBehaviour(rightNeighbor))
        {
            damageAmount += extraDamageAmount;
            attackingUnit.DisplayFloatingText("BONUS ATK", 1);
            return true;
        }

        return false;
    }
}
