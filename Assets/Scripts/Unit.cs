using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "ScriptableObjects/Unit", order = 1)]
public class Unit : ScriptableObject
{
    public enum Tribe
    {
        Red,
        Blue,
        Green,
        Yellow
    }

    public enum Alignment
    {
        Rock,
        Paper,
        Scissors
    }

    public Tribe tribe;
    public Alignment alignment;
    public int hp;
    public int attack;
    public Sprite unitSprite;
}
