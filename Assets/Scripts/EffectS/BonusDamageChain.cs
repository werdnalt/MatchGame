using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BonusDamageChain", menuName = "ScriptableObjects/Effects/BonusDamageChain", order = 2)]
public class BonusDamageChain : Effect
{
    public int damagePer3Chain;
    public int damagePerMoreChain;
    public override void OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        var chainSize = BoardManager.Instance.GetChainedUnits(attackedUnit).Count;

        if (chainSize == 3) damageAmount += damagePer3Chain;
        if (chainSize > 3) damageAmount += damagePerMoreChain;
    }
}
