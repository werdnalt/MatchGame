using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public enum Type
    {
        Sword,
        Arrow,
        Shield,
        Fire,
        Water,
        Potion,
        Enemy_Effect,
        Invincible,
        Leaf, 
        Coin
    }

    public Type blockType;
    public int pointValue;
}
