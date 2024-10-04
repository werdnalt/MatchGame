using UnityEngine;

[CreateAssetMenu(fileName = "HealOnKillEffect", menuName = "ScriptableObjects/Effects/HealOnKillEffect", order = 5)]
public class HealOnKillEffect : Effect
{
    public int amountToHeal;

    public override bool OnKill(UnitBehaviour killedBy, UnitBehaviour killed)
    {
        killedBy.Heal(amountToHeal);
        return true;
    }
}
