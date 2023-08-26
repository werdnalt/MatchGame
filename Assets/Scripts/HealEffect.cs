using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "ScriptableObjects/Effects/HealEffect", order = 2)]
public class HealEffect : Effect
{
    public int healAmount;

    public override void OnDeath()
    {
        if (!BoardManager.Instance.mostRecentlyAttackingUnit) return;
        
        BoardManager.Instance.mostRecentlyAttackingUnit.Heal(healAmount);
    }
}
