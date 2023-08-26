using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UltimateHealEffect", menuName = "ScriptableObjects/Effects/UltimateHealEffect", order = 2)]
public class UltimateHealEffect : Effect
{
    public int healAmount;
    
    public override void OnAttack()
    {
        var unitToLeftCoords = BoardManager.Instance.GetNeighborCoordinates(_unit.coordinates, BoardManager.Direction.Left);
        var unitToLeft = BoardManager.Instance.GetUnitBehaviourAtCoordinate(unitToLeftCoords);
        
        var unitToRightCoords = BoardManager.Instance.GetNeighborCoordinates(_unit.coordinates, BoardManager.Direction.Right);
        var unitToRight = BoardManager.Instance.GetUnitBehaviourAtCoordinate(unitToRightCoords);
        
        if (unitToLeft) unitToLeft.Heal(healAmount);
        if (unitToRight) unitToRight.Heal(healAmount);
        
        _unit.Heal(healAmount);
    }
}
