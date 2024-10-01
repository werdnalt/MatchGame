using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UltimateHealEffect", menuName = "ScriptableObjects/Effects/UltimateHealEffect", order = 2)]
public class UltimateHealEffect : Effect
{
    public int healAmount;
    
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        damageAmount = damageAmount;

        var attackingUnitCoordinates = BoardManager.Instance.GetCoordinatesForUnitBehaviour(attackingUnit);
        var attackedUnitCoordinates = BoardManager.Instance.GetCoordinatesForUnitBehaviour(attackedUnit);
        
        var unitToLeftCoords = BoardManager.Instance.GetSingleNeighborCoordinates(attackingUnitCoordinates.Value, BoardManager.Direction.Left);
        var unitToLeft = BoardManager.Instance.GetUnitBehaviour(unitToLeftCoords);
        
        var unitToRightCoords = BoardManager.Instance.GetSingleNeighborCoordinates(attackedUnitCoordinates.Value, BoardManager.Direction.Right);
        var unitToRight = BoardManager.Instance.GetUnitBehaviour(unitToRightCoords);
        
        if (unitToLeft) unitToLeft.Heal(healAmount);
        if (unitToRight) unitToRight.Heal(healAmount);
        
        attackingUnit.Heal(healAmount);

        return true;
    }
}
