using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealRightNeighborAndSelfEffect", menuName = "ScriptableObjects/Effects/HealRightNeighborAndSelfEffect", order = 5)]

public class HealRightNeighborAndSelfEffect : Effect
{
    public int healAmount;
    
    public override bool OnKill(UnitBehaviour killedBy, UnitBehaviour killed)
    {
        killedBy.Heal(healAmount);
        var rightNeighbor = BoardManager.Instance.GetUnitBehaviour(
            BoardManager.Instance.GetSingleNeighborCoordinates(killedBy.currentCoordinates,
                BoardManager.Direction.Right));
        if (rightNeighbor) rightNeighbor.Heal(healAmount);
        return true;
    }
}
