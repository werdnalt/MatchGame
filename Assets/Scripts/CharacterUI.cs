using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    public TextMeshProUGUI attackText;
    public Image healthBar;
    public Image characterSprite;

    public Character character;
    
    public void RefreshUI()
    {
        healthBar.fillAmount = character.hp;
    }
}
