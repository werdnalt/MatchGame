using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "ScriptableObjects/Unit", order = 1)]
public class Unit : ScriptableObject
{
    public enum Tribe
    {
        Hero,
        Red,
        Blue,
        Plants,
        Beasts,
        Void,
        NA
    }

    public GameObject animatedCharacterPrefab;
    public int attackTimer;
    public bool shouldAnimateLoop;
    public Tribe tribe;
    public int hp;
    public int attack;
    public List<Effect> effects;
    public int attackRange;
    public Sprite unitSprite;
    public string displayName;
}
