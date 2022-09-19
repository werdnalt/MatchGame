using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    public Image characterSprite;
    public Image specialAbilitySprite;
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
