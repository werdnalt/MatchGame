using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StealEffectsEffect", menuName = "ScriptableObjects/Effects/StealEffectsEffect", order = 2)]
public class StealEffectsEffect : Effect
{
    public override void OnSwap(UnitBehaviour swappedUnit, UnitBehaviour swappedWith)
    {
        if (!swappedWith || swappedWith.effects.Count == 0) return;
        
        for (var i = swappedWith.effects.Count - 1; i >= 0; i--)
        {
            var effectState = swappedWith.effects[i];
            swappedUnit.effects.Add(effectState);
            swappedWith.effects.Remove(effectState);
        }
    }
}
