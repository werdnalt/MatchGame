using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StealEffectsEffect", menuName = "ScriptableObjects/Effects/StealEffectsEffect", order = 2)]
public class StealEffectsEffect : Effect
{
    public override bool OnSwap(Coordinates swappedUnitCoordinates, Coordinates swappedWithCoordinates)
    {
        var swappedUnit = BoardManager.Instance.GetUnitBehaviour(swappedUnitCoordinates);
        var swappedWith = BoardManager.Instance.GetUnitBehaviour(swappedWithCoordinates);
        
        if (!swappedWith || !swappedUnit || swappedWith.effectStates.Count == 0) return false;
        
        for (var i = swappedWith.effectStates.Count - 1; i >= 0; i--)
        {
            var effectState = swappedWith.effectStates[i];
            swappedUnit.effectStates.Add(effectState);
            swappedWith.effectStates.Remove(effectState);
        }

        return true;
    }
}
