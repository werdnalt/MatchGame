using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseHealthEffect", menuName = "ScriptableObjects/Effects/IncreaseHealthEffect", order = 2)]
public class IncreaseHealthEffect : Effect
{
    public int increaseAmount;
    
    public override bool OnSwap(Coordinates swappedUnitCoordinates, Coordinates swappedWithCoordinates)
    {
        var swappedUnit = BoardManager.Instance.GetUnitBehaviour(swappedUnitCoordinates);
        if (!swappedUnit) return false;
        
        swappedUnit.IncreaseHealth(increaseAmount);
        //UIManager.Instance.ShowUnitPanel(swappedUnit);

        return true;
    }
}
