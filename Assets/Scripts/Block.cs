using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public enum Type
    {
        Sword,
        Bow,
        Shield,
        Fireball,
        Star,
        Potion,
        Leaf, 
        Coin,
        Invincible,
        Enemy_Effect,
        Skull
    }

    public Type blockType;
    public int pointValue;
    [SerializeField] private SpriteRenderer _blockIcon;

    private void Start()
    {
        _blockIcon = GetComponent<SpriteRenderer>();
        Sprite loadedIcon = Resources.Load<Sprite>(blockType.ToString());
        _blockIcon.sprite = loadedIcon ? loadedIcon : Resources.Load<Sprite>("default");
    }
}
