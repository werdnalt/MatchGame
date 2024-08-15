using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class HeroTreasureChoice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public UnitBehaviour hero;
    public Treasure treasure;

    public Image heroPortrait;

    private Vector3 _originalScale;

    private void Start()
    {
        _originalScale = transform.localScale;
    }

    public void Setup(UnitBehaviour heroUnit, Treasure chosenTreasure)
    {
        hero = heroUnit;
        treasure = chosenTreasure;
        heroPortrait.sprite = hero.unitData.unitSprite;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.localScale = _originalScale;
        var newScale = new Vector3(_originalScale.x * 1.2f, _originalScale.y * 1.2f, 1);
        transform.DOScale(newScale, .5f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(_originalScale, .5f).SetEase(Ease.OutQuad);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        hero.GiveTreasure(treasure);
        UIManager.Instance.HideTreasurePopup();
    }
}