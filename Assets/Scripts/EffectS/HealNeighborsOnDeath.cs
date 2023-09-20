using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealNeighborsOnDeath", menuName = "ScriptableObjects/Effects/HealNeighborsOnDeath", order = 4)]
public class HealNeighborsOnDeath : Effect
{
    public int healAmount;
    
    public override void OnDeath(UnitBehaviour killedBy, UnitBehaviour killed)
    {
        var unitCoordinates = killed.coordinates;
        var neighbors = BoardManager.Instance.GetAllNeighboringCoordinates(unitCoordinates);
        foreach (var neighbor in neighbors)
        {
            var unit = BoardManager.Instance.GetUnitBehaviourAtCoordinate(neighbor);
            if (unit) unit.Heal(healAmount);
        }
    }
}
