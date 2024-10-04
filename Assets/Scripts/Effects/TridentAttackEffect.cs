using UnityEngine;

[CreateAssetMenu(fileName = "TridentAttackEffect", menuName = "ScriptableObjects/Effects/TridentAttackEffect", order = 4)]

public class TridentAttackEffect : Effect
{
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        attackingUnit.canChainAttack = false;
        var leftNeighborCoordinates =
            BoardManager.Instance.GetSingleNeighborCoordinates(attackedUnit.currentCoordinates,
                BoardManager.Direction.Left);
        var leftNeighbor = BoardManager.Instance.GetUnitBehaviour(leftNeighborCoordinates);
        if (leftNeighbor) leftNeighbor.TakeDamage(damageAmount, attackingUnit);
        
        var rightNeighborCoordinates =
            BoardManager.Instance.GetSingleNeighborCoordinates(attackedUnit.currentCoordinates,
                BoardManager.Direction.Right);
        var rightNeighbor = BoardManager.Instance.GetUnitBehaviour(rightNeighborCoordinates);
        if (rightNeighbor) rightNeighbor.TakeDamage(damageAmount, attackingUnit);
        
        return true;
    }

    public override void RemoveEffect(UnitBehaviour unitBehaviour)
    {
        unitBehaviour.canChainAttack = true;
    }
}
