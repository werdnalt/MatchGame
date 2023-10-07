using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChestEffect", menuName = "ScriptableObjects/Effects/ChestEffect", order = 2)]
public class ChestEffect : Effect
{
    public List<Treasure> treasures;
    
    public override void OnDeath(UnitBehaviour killedBy, UnitBehaviour killed)
    {
        UIManager.Instance.ChestDestroyed(treasures);
    }
}
