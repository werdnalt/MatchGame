using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "AbsorbNeighborEffect", menuName = "ScriptableObjects/Effects/AbsorbNeighborEffect", order = 5)]
public class AbsorbNeighbor : Effect
{
    public override void OnSwap(Coordinates swappedUnitCoordinates, Coordinates swappedByUnitCoordinates)
    {
        var swappedUnit = BoardManager.Instance.GetUnitBehaviour(swappedUnitCoordinates);
        var swappedWith = BoardManager.Instance.GetUnitBehaviour(swappedByUnitCoordinates);
        
        if (!swappedWith || !swappedUnit) return;
        
        swappedUnit.attack += swappedWith.attack;
        swappedUnit.currentHp += swappedWith.currentHp;
        swappedUnit.ShowAndUpdateHealth();
        //UIManager.Instance.ShowUnitPanel(swappedUnit);
        BoardManager.Instance.RemoveUnitBehaviour(swappedWith);
    }
    
}
