using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectState
{
    public Effect effect;
    public int numUses;
    public int currUses;

    public EffectState(Effect effect)
    {
        this.effect = effect;
        numUses = effect.numUses;
    }
}
