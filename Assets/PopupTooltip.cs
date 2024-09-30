using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupTooltip : MonoBehaviour
{
    public Image nameplateImage;
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI healthAmountText;
    public TextMeshProUGUI attackAmountText;
    public TextMeshProUGUI rangeAmountText;
    public TextMeshProUGUI timerText;
    public Image timerImage;
    public List<Keyword> keywords = new List<Keyword>();
    
    [SerializeField] private GameObject effectTextPrefab;
    [SerializeField] private GameObject effectTextParent;
    
    [Serializable]
    public struct Keyword
    {
        public string pattern;
        public string spriteName; 
        public Color highlightColor; 
    }
    
    private List<GameObject> _instantiatedEffectPrefabs = new List<GameObject>();
    [SerializeField] private PopEffect _popEffect;
    
    public void ShowUnitPanel(UnitBehaviour unitBehaviour)
    {
        for (var i = _instantiatedEffectPrefabs.Count - 1; i >= 0; i--)
        {
            Destroy(_instantiatedEffectPrefabs[i].gameObject);
        }
        
        _popEffect.EnableAndPop();


        switch (unitBehaviour.unitData.tribe)
        {
            case Unit.Tribe.Beasts:
                nameplateImage.color = new Color32(249, 194, 43, 255);
                break;
            case Unit.Tribe.Void:
                nameplateImage.color = new Color32(107,62,117, 255);
                break;
            case Unit.Tribe.Plants:
                nameplateImage.color = new Color32(213,224,75, 255);
                break;
        }
        
        attackAmountText.text = unitBehaviour.attack.ToString();
        healthAmountText.text = ($"{unitBehaviour.currentHp}");
        rangeAmountText.text = $"{unitBehaviour.unitData.attackRange}";
        if (unitBehaviour is EnemyUnitBehaviour)
        {
            timerImage.gameObject.SetActive(true);
            var enemyUnitBehaviour = unitBehaviour as EnemyUnitBehaviour;
            timerText.text = $"{enemyUnitBehaviour.GetAttackTimerAmount()}";
        }
        else
        {
            timerImage.gameObject.SetActive(false);
            timerText.text = $"";
        }
        unitNameText.text = unitBehaviour.unitData.displayName;
        
        foreach (var effectState in unitBehaviour.effects)
        {
            var effectTextInstance = Instantiate(effectTextPrefab, effectTextParent.transform);
            var effectText = HighlightKeywords(effectState.effect.effectDescription);
            effectTextInstance.GetComponentInChildren<TextMeshProUGUI>().text = effectText;
            
            if (effectState.isSilenced)
            {
                effectTextInstance.GetComponentInChildren<TextMeshProUGUI>().text =
                    ($"<s>{effectText}</s>");
            }
            
            _instantiatedEffectPrefabs.Add(effectTextInstance);
        }
    }
    
    private string HighlightKeywords(string text)
    {
        // Check if text is null and return an empty string if so
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError("HighlightKeywords: Input text is null or empty.");
            return string.Empty;
        }

        foreach (var keyword in keywords)
        {
            // Use the pattern directly for regex matching
            var pattern = keyword.pattern;
        
            // Create a color string in hex format
            string colorHex = ColorUtility.ToHtmlStringRGB(keyword.highlightColor);

            // Prepare the sprite insertion if the sprite name is not empty
            var spriteInsertion = string.Empty;
        
            // Assuming the spriteName corresponds to an index in the TMP sprite asset
            if (!string.IsNullOrEmpty(keyword.spriteName))
            {
                // Assuming spriteName is the index number of the sprite in the sprite asset
                spriteInsertion = $"{keyword.spriteName}";
            }

            // Highlight the matches with the specified color and insert sprite
            var replacement = $"<color=#{colorHex}>$&</color>{spriteInsertion}"; // $& represents the entire match
            text = Regex.Replace(text, pattern, replacement);
        }
        return text;
    }

}
