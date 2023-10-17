using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SwapStatsEffect", menuName = "ScriptableObjects/Effects/SwapStatsEffect", order = 2)]
public class SwapStatsEffect : Effect
{
    public override void OnSwap(UnitBehaviour swappedUnit, UnitBehaviour swappedWith)
    {
        if (swappedWith == null) return;
        
        (swappedWith.currentHp, swappedWith.attack) = (swappedWith.attack, swappedWith.currentHp);
        swappedWith.ShowAndUpdateHealth();
        swappedWith.ShowAttack();
    }
}
