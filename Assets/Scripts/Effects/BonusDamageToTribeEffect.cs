using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BonusDamageToTribeEffect", menuName = "ScriptableObjects/Effects/BonusDamageToTribeEffect", order = 4)]

public class BonusDamageToTribeEffect : Effect
{
    public int extraDamage;

    private Unit.Tribe bonusTribe;

    public override void SetUnitBehaviour(UnitBehaviour unitBehaviour)
    {
        base.SetUnitBehaviour(unitBehaviour);
        ChooseBonusTribe();
    }
    
    public override bool OnAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit, ref int damageAmount)
    {
        if (attackedUnit.unitData.tribe == bonusTribe)
        {
            damageAmount += extraDamage;
            attackingUnit.DisplayFloatingText("BONUS DAMAGE!", 1);
            ChooseBonusTribe();
            return true;
        }
        
        return false;
    }

    private void ChooseBonusTribe()
    {
        var tribes = new List<Unit.Tribe>();
        tribes.Add(Unit.Tribe.Beasts);
        tribes.Add(Unit.Tribe.Skeleton);
        tribes.Add(Unit.Tribe.Goblin);
        tribes.Add(Unit.Tribe.Slime);
        
        bonusTribe = tribes[Random.Range(0, tribes.Count)];
        effectDescription = $"ON ATTACK: +{extraDamage} ATK to {bonusTribe} Goons";
    }
}
