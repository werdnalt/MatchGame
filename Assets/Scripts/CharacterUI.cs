using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    public Image characterSprite;
    public Image specialAbilitySprite;
    public Image specialAbilityHider;
    public Image bombSprite;
    public List<Image> hearts;

    private Material characterMaterial;
    private int _heartIndex = 2;

    public Character character;

    public void Setup(Character toSetup)
    {
        characterSprite.sprite = toSetup.characterSprite;
        specialAbilitySprite.sprite = toSetup.specialBlockSprite;

        characterMaterial = Instantiate(characterSprite.material);
        characterSprite.material = characterMaterial;
    }
    public void RefreshUI()
    {
    }

    public void UpdateSpecialAbilityBar(int currentAbilityAmount, int maxAbilityAmount)
    {
        specialAbilityHider.fillAmount = (float)(maxAbilityAmount - currentAbilityAmount) / maxAbilityAmount;
    }

    public void UpdateBombUI(int bombPoints)
    {
        if (bombPoints < 50)
        {
            bombSprite.sprite = Resources.Load<Sprite>("Skull");
        }
        
        if (bombPoints >= 50 && bombPoints <= 150)
        {
            bombSprite.sprite = Resources.Load<Sprite>("landmine");
        }
        
        if (bombPoints >150 && bombPoints <= 300)
        {
            bombSprite.sprite = Resources.Load<Sprite>("round");
        }
        
        if (bombPoints > 300)
        {
            bombSprite.sprite = Resources.Load<Sprite>("atomic");
        }
    }

    public void DamageFlash()
    {
        hearts[_heartIndex].enabled = false;
        _heartIndex--;
        StartCoroutine(IDamageFlash());
    }

    private IEnumerator IDamageFlash()
    {
        int numFlashes = 5;
        float flashDuration = .1f;
        for (int i = 0; i < numFlashes; i++)
        {
            characterMaterial.SetFloat("_HitEffectBlend", 1);
            yield return new WaitForSeconds(flashDuration);
            characterMaterial.SetFloat("_HitEffectBlend", 0);
            yield return new WaitForSeconds(flashDuration);
        }
    }

    public void SetDead()
    {
        characterMaterial.SetFloat("_GreyscaleBlend", 1);
    }
}
