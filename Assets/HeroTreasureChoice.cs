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

    public void Setup(UnitBehaviour heroUnit, Treasure chosenTreasure)
    {
        hero = heroUnit;
        treasure = chosenTreasure;
        heroPortrait.sprite = hero.unitData.unitSprite;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        hero.GiveTreasure(treasure);
    }
}
