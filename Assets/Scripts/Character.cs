using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Character", order = 1)]
public class Character : ScriptableObject
{
    public string id;
    public int hp;
    public int specialAbilityRequirement;
    public GameObject cursor;
    public GameObject specialBlockPrefab;
    public CharacterUI characterUI;
    public Sprite characterSprite;
    public Sprite specialBlockSprite;
    public string specialBlockDescription;
}
