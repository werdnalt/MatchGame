using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/EnemyHydrator", order = 1)]
public class EnemyHydrator : ScriptableObject
{
    public Type type;
    public int hp;
    public float attackTime;

    public enum Type 
    {
        Boar,
        Goblin,
        Orc,
        Dragon
    }
}
