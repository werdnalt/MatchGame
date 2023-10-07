using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Treasure", menuName = "ScriptableObjects/Treasure", order = 4)]
public class Treasure : ScriptableObject
{
    public Effect effect;
    public string treasureDescription;
    public Sprite treasureSprite;
}
