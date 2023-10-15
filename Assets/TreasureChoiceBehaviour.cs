using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TreasureChoiceBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Treasure treasure;
    public TextMeshProUGUI treasureDescriptionText;
    public Image treasureSprite;
    public TextMeshProUGUI treasureName;

    private Vector3 _startingScale;

    [SerializeField] private Vector3 chosenTreasurePosition;
    [SerializeField] private ScaleAndShake scaleAndShake;
    [SerializeField] private GameObject treasureObject;

    private void Start()
    {
        EventManager.Instance.onTreasureChosen += HideSelf;
        _startingScale = transform.localScale;
    }

    public void SetTreasure(Treasure t)
    {
        treasure = t;
        treasureSprite.sprite = t.treasureSprite;
        treasureName.text = t.name;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        treasureObject.transform.DOKill();
        treasureObject.transform.localScale = _startingScale;
        var scaleFactor = 1.3f;
        var newScale = new Vector3(_startingScale.x * scaleFactor, _startingScale.y * scaleFactor, 1);
        treasureObject.transform.DOScale(newScale, .25f).SetEase(Ease.OutQuad);

        treasureDescriptionText.text = treasure.treasureDescription;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        treasureObject.transform.DOScale(_startingScale, .25f).SetEase(Ease.OutQuad);
        treasureDescriptionText.text = "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        scaleAndShake.Animate();
        EventManager.Instance.ChooseTreasure(this.gameObject);
        transform.DOMove(chosenTreasurePosition, 1f).SetEase(Ease.OutQuad).OnComplete(()=>
        {
            UIManager.Instance.chosenTreasure = treasure;
            UIManager.Instance.ShowHeroChoices();
        });
    }

    public void HideSelf(GameObject t)
    {
        if (t != this.gameObject)
        {
            gameObject.SetActive(false);
        }
    }
}
