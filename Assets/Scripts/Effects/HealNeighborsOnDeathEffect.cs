using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealNeighborsOnDeathEffect", menuName = "ScriptableObjects/Effects/HealNeighborsOnDeathEffect", order = 4)]

public class HealNeighborsOnDeathEffect : Effect
{
    public int healAmount;
    
    public override void OnDeath(UnitBehaviour killedBy, UnitBehaviour killed)
    {
        var allNeighbors = BoardManager.Instance.GetAllNeighborUnitBehaviours(killed);
        foreach (var neighbor in allNeighbors)
        {
            if (neighbor != null && !neighbor.isDead )
            {
                neighbor.Heal(healAmount);
            }
        }
    }
}
