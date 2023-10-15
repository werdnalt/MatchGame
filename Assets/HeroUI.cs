using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HeroUI : MonoBehaviour
{
    [SerializeField] private Image heroPortrait;
    [SerializeField] private TextMeshProUGUI heroName;
    [SerializeField] private TextMeshProUGUI healthAmountText;
    [SerializeField] private TextMeshProUGUI attackAmountText;
    [SerializeField] private Image fullHeart;
    [SerializeField] private Image swordImage;
    [SerializeField] private GameObject healthUI;
    [SerializeField] private GameObject treasureUIPrefab;
    [SerializeField] private GameObject treasureUIParent;

    private UnitBehaviour _unitBehaviour;
    private bool _heartSizeSet;
    public Vector3 currentHeartSize;

    private List<TreasureUI> _treasureUIs;
    
    public void Setup(UnitBehaviour unitBehaviour)
    {
        heroPortrait.sprite = unitBehaviour.unitData.unitSprite;
        heroName.text = unitBehaviour.unitData.name;
        _unitBehaviour = unitBehaviour;

        healthAmountText.text = unitBehaviour.currentHp.ToString();
        attackAmountText.text = unitBehaviour.attack.ToString();
    }

    public void TakeDamage()
    {
        if (_unitBehaviour.isDead) return;
        
        if (_unitBehaviour.currentHp > _unitBehaviour._maxHp)
        {
            healthAmountText.color = new Color32(39, 246, 81, 255);
            fullHeart.fillAmount = 100;
        }
        
        if (_unitBehaviour.currentHp == _unitBehaviour._maxHp)
        {
            fullHeart.fillAmount = 100;
        }
        
        if (_unitBehaviour.currentHp < _unitBehaviour._maxHp)
        {
            fullHeart.fillAmount = ((float)_unitBehaviour.currentHp / _unitBehaviour._maxHp);
        }

        healthAmountText.text = _unitBehaviour.currentHp.ToString();
        
        // play animation
        healthUI.transform.DOKill();
        if (!_heartSizeSet)
        {
            currentHeartSize = healthUI.transform.localScale;
            _heartSizeSet = true;
        }

        healthUI.transform.DOKill();
        healthUI.transform.localScale = currentHeartSize;
        var newHeartSize = new Vector3(currentHeartSize.x * 1.2f, currentHeartSize.y * 1.2f, 1);
        healthUI.transform.DOPunchScale(newHeartSize, .25f, 0, .3f).SetEase(Ease.OutQuad);
    }

    public void AddTreasureUI(Treasure treasure)
    {
        var treasureUIInstance = Instantiate(treasureUIPrefab, treasureUIParent.transform);
        treasureUIInstance.GetComponent<TreasureUI>().Setup(treasure);
    }

    public void DestroyTreasureUI()
    {
        
    }

    public void ModifyAttack()
    {
        
    }

    public void CountdownTreasure(Treasure treasure)
    {
        var treasureUI = GetTreasure(treasure);
        if (treasureUI) treasureUI.Countdown();
    }

    private TreasureUI GetTreasure(Treasure treasure)
    {
        foreach (var treasureUI in _treasureUIs)
        {
            if (treasureUI.treasure == treasure)
                return treasureUI;
        }

        return null;
    }
}
