using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectState
{
    public Effect effect;
    public int numUses;
    public int currUses;
    public bool isSilenced;

    public EffectState(Effect effect)
    {
        this.effect = effect;
        numUses = effect.numUses;
    }

    public bool isDepleted()
    {
        currUses++;
        return currUses >= numUses;
    }

    public void Silence()
    {
        isSilenced = true;
    }
}
