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
        Green,
        Yellow,
        Purple
    }

    public enum Classification
    {
        Rock,
        Paper,
        Scissors
    }

    private void AnimateSprite()
    {
        
    }

    public GameObject animatedCharacterPrefab;
    public int attackTimer;
    public bool shouldAnimateLoop;
    public Tribe tribe;
    public Classification classification;
    public int hp;
    public int attack;
    public int experienceAmount;
    public Sprite unitSprite;
    public List<Effect> effects;
    public bool passive;
    public string displayName;

}
