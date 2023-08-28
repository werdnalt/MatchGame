using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExperienceEffect", menuName = "ScriptableObjects/Effects/ExperienceEffect", order = 1)]
public class ExperienceEffect : Effect
{
    public int experienceAmount;
    public override void OnDeath(UnitBehaviour killedBy, UnitBehaviour killed)
    {
        killedBy.GainExperience(experienceAmount);
    }
}
