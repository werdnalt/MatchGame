using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "AbsorbNeighborEffect", menuName = "ScriptableObjects/Effects/AbsorbNeighborEffect", order = 5)]
public class AbsorbNeighbor : Effect
{
    public override void OnSwap(UnitBehaviour swappedUnit, UnitBehaviour swappedWith)
    {
        if (!swappedWith) return;
        
        swappedUnit.attack += swappedWith.attack;
        swappedUnit.currentHp += swappedWith.currentHp;
        swappedWith.transform.DOKill();
        swappedUnit.UpdateHearts();
        UIManager.Instance.ShowUnitPanel(swappedUnit);
        BoardManager.Instance.RemoveUnitFromBoard(swappedWith);
    }
    
}