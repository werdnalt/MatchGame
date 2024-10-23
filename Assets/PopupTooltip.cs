using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PopupTooltip : MonoBehaviour
{
    public Image nameplateImage;
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI healthAmountText;
    public TextMeshProUGUI attackAmountText;
    public TextMeshProUGUI armorAmountText;
    public TextMeshProUGUI rangeAmountText;
    public TextMeshProUGUI timerText;
    public GameObject armorObject;
    public GameObject timerObject;
    

    [FormerlySerializedAs("badImage")] [SerializeField] private Image badgeImage;
    [SerializeField] private GameObject effectTextPrefab;
    [SerializeField] private GameObject effectTextParent;

    [SerializeField] private Sprite beastBadge;
    [SerializeField] private Sprite goblinBadge;
    [SerializeField] private Sprite skeletonBadge;
    [SerializeField] private Sprite slimeBadge;
    [SerializeField] private Sprite heroBadge;
    
    private List<GameObject> _instantiatedEffectPrefabs = new List<GameObject>();
    private TextHighlighter _textHighlighter;
    [SerializeField] private PopEffect _popEffect;

    private void Start()
    {
        _textHighlighter = TextHighlighter.Instance;
    }

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
                nameplateImage.color = new Color32(147, 66, 141, 255);
                badgeImage.sprite = beastBadge;
                break;
            case Unit.Tribe.Goblin:
                nameplateImage.color = new Color32(175,94,98, 255);
                badgeImage.sprite = goblinBadge;
                break;
            case Unit.Tribe.Slime:
                nameplateImage.color = new Color32(189,216,128, 255);
                badgeImage.sprite = slimeBadge;
                break;
            case Unit.Tribe.Skeleton:
                nameplateImage.color = new Color32(155,221,206, 255);
                badgeImage.sprite = skeletonBadge;
                break;
            case Unit.Tribe.Hero:
                nameplateImage.color = new Color32(230,244,241, 255);
                badgeImage.sprite = heroBadge;
                break;
        }
        
        attackAmountText.text = unitBehaviour.attack.ToString();
        healthAmountText.text = ($"{unitBehaviour.currentHp}");
        rangeAmountText.text = $"{unitBehaviour.attackRange}";
        if (unitBehaviour is EnemyUnitBehaviour)
        {
            timerObject.gameObject.SetActive(true);
            var enemyUnitBehaviour = unitBehaviour as EnemyUnitBehaviour;
            timerText.text = $"{enemyUnitBehaviour.GetAttackTimerAmount()}";
        }
        else
        {
            timerObject.gameObject.SetActive(false);
            timerText.text = $"";
        }

        if (unitBehaviour.armor > 0)
        {
            armorObject.SetActive(true);
            armorAmountText.text = unitBehaviour.armor.ToString();
        }
        else
        {
            armorObject.SetActive(false);
        }
        
        unitNameText.text = unitBehaviour.unitData.displayName;
        
        foreach (var effectState in unitBehaviour.effects)
        {
            var effectTextInstance = Instantiate(effectTextPrefab, effectTextParent.transform);
            var effectText = _textHighlighter.HighlightKeywords(effectState.effect.effectDescription);
            effectTextInstance.GetComponentInChildren<TextMeshProUGUI>().text = effectText;
            
            if (effectState.isSilenced)
            {
                effectTextInstance.GetComponentInChildren<TextMeshProUGUI>().text =
                    ($"<s>{effectText}</s>");
            }
            
            _instantiatedEffectPrefabs.Add(effectTextInstance);
        }
        
        foreach (var statusEffect in unitBehaviour.ongoingStatuses)
        {
            var effectTextInstance = Instantiate(effectTextPrefab, effectTextParent.transform);
            var statusDescription = $"{statusEffect.statusEffect.ToString()}: {statusEffect.actionsLeft} Actions Left";
            var effectText = _textHighlighter.HighlightKeywords(statusDescription);
            effectTextInstance.GetComponentInChildren<TextMeshProUGUI>().text = effectText;
            _instantiatedEffectPrefabs.Add(effectTextInstance);
        }
    }
    
    public void ShowUnitPanel(Unit unit)
    {
        for (var i = _instantiatedEffectPrefabs.Count - 1; i >= 0; i--)
        {
            Destroy(_instantiatedEffectPrefabs[i].gameObject);
        }
        
        _popEffect.EnableAndPop();


        switch (unit.tribe)
        {
            case Unit.Tribe.Beasts:
                nameplateImage.color = new Color32(249, 194, 43, 255);
                break;
        }
        
        attackAmountText.text = unit.attack.ToString();
        healthAmountText.text = ($"{unit.hp}");
        rangeAmountText.text = $"{unit.attackRange}";
        unitNameText.text = unit.displayName;
        
        foreach (var effect in unit.effects)
        {
            var effectTextInstance = Instantiate(effectTextPrefab, effectTextParent.transform);
            var effectText = _textHighlighter.HighlightKeywords(effect.effectDescription);
            effectTextInstance.GetComponentInChildren<TextMeshProUGUI>().text = effectText;
            _instantiatedEffectPrefabs.Add(effectTextInstance);
        }
    }
    
    

    public void Hide()
    {
        _popEffect.DisableAndPop();
    }

}
