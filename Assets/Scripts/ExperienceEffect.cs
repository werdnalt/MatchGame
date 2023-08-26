using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExperienceEffect", menuName = "ScriptableObjects/Effects/ExperienceEffect", order = 1)]
public class ExperienceEffect : Effect
{
    public int experienceAmount;
    public override void OnDeath()
    {
        if (!BoardManager.Instance.mostRecentlyAttackingUnit) return;
        
        Debug.Log($"Awarding {BoardManager.Instance.mostRecentlyAttackingUnit} {_unit.unit.experienceAmount} experience");
        BoardManager.Instance.mostRecentlyAttackingUnit.GainExperience(experienceAmount);
    }
}
