using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseHealthEffect", menuName = "ScriptableObjects/Effects/IncreaseHealthEffect", order = 2)]
public class IncreaseHealthEffect : Effect
{
    public int increaseAmount;
    
    public override void OnSwap(UnitBehaviour swappedUnit, UnitBehaviour swappedWith)
    {
        swappedUnit.IncreaseHealth(increaseAmount);
        UIManager.Instance.ShowUnitPanel(swappedUnit);
    }
}
