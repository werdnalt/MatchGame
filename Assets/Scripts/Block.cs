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
        Skull,
        Sticky,
        Bomb,
        Wall,
        Boulder
    }

    public Type blockType;
    public int pointValue;
    [SerializeField] private SpriteRenderer _blockIcon;
    private Renderer _renderer;

    private void Start()
    {
        _blockIcon = GetComponent<SpriteRenderer>();
        _renderer = GetComponent<Renderer>();
        Sprite loadedIcon = Resources.Load<Sprite>(blockType.ToString());
        _blockIcon.sprite = loadedIcon ? loadedIcon : Resources.Load<Sprite>("default");
    }

    public void Match()
    {
        StartCoroutine(Flash());
    }

    private IEnumerator Matched()
    {
        //_renderer.material.SetFloat("_HitEffectBlend", 0);
        StartCoroutine(Flash());
        yield return new WaitForSeconds(1f);
        
    }

    private IEnumerator Flash()
    {
        int numFlashes = 5;
        float flashDuration = .1f;
        for (int i = 0; i < numFlashes; i++)
        {
            _blockIcon.material.SetFloat("_HitEffectBlend", 1);
            yield return new WaitForSeconds(flashDuration);
            _blockIcon.material.SetFloat("_HitEffectBlend", 0);
            yield return new WaitForSeconds(flashDuration);
        }
        Destroy(gameObject);
    }

    public void OnSelectorHover()
    {
        if (blockType == Type.Bomb)
        {
            
        }

        if (blockType == Type.Sticky)
        {
            
        }
    }
    
}
