using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UltimateHealEffect", menuName = "ScriptableObjects/Effects/UltimateHealEffect", order = 2)]
public class UltimateHealEffect : Effect
{
    public int healAmount;
    
    public override void OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        damageAmount = damageAmount;
        
        var unitToLeftCoords = BoardManager.Instance.GetSingleNeighborCoordinates(attackingUnit.coordinates, BoardManager.Direction.Left);
        var unitToLeft = BoardManager.Instance.GetUnitBehaviour(unitToLeftCoords);
        
        var unitToRightCoords = BoardManager.Instance.GetSingleNeighborCoordinates(attackingUnit.coordinates, BoardManager.Direction.Right);
        var unitToRight = BoardManager.Instance.GetUnitBehaviour(unitToRightCoords);
        
        if (unitToLeft) unitToLeft.Heal(healAmount);
        if (unitToRight) unitToRight.Heal(healAmount);
        
        attackingUnit.Heal(healAmount);
    }
}
