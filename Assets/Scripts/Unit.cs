using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "ScriptableObjects/Unit", order = 1)]
public class Unit : ScriptableObject
{
    public enum Tribe
    {
        Hero,
        Beasts,
        NA,
        Goblin,
        Slime,
        Skeleton
    }

    public GameObject animatedCharacterPrefab;
    public int attackTimer;
    public Tribe tribe;
    public int hp;
    public int armor;
    public int attack;
    public List<Effect> effects;
    public int attackRange;
    public Sprite unitSprite;
    public string displayName;
    public bool cantChain;
    public string flavorText;
}
