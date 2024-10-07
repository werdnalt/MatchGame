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
    public Image treasureBG;

    private Vector3 _objectInitialScale;
    private Vector3 _spriteInitialScale;

    [SerializeField] private Vector3 chosenTreasurePosition;
    [SerializeField] private ScaleAndShake scaleAndShake;
    [SerializeField] private GameObject treasureObject;

    private bool canShowDescription;

    private void Awake()
    {
        EventManager.Instance.onTreasureChosen += HideSelf;
        EventManager.Instance.onTreasureChosen += DisableDescription;
        _spriteInitialScale = treasureSprite.transform.localScale;
        _objectInitialScale = transform.localScale;
    }

    private void OnEnable()
    {
        canShowDescription = true;
        transform.localScale = _objectInitialScale;
        treasureSprite.transform.localScale = _spriteInitialScale;
        treasureBG.gameObject.SetActive(true);
    }

    private void DisableDescription(GameObject gameObject)
    {
        canShowDescription = false;
    }

    public void SetTreasure(Treasure t)
    {
        treasure = t;
        treasureSprite.sprite = t.treasureSprite;
        treasureName.text = t.name;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        treasureSprite.transform.DOKill();
        treasureSprite.transform.localScale = _spriteInitialScale;
        var scaleFactor = 1.3f;
        var newScale = new Vector3(_spriteInitialScale.x * scaleFactor, _spriteInitialScale.y * scaleFactor, 1);
        treasureSprite.transform.DOScale(newScale, .25f).SetEase(Ease.OutQuad);

        if (canShowDescription)
        {
            treasureDescriptionText.text = treasure.treasureDescription;
            treasureName.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        treasureName.gameObject.SetActive(false);
        treasureSprite.transform.DOScale(_spriteInitialScale, .25f).SetEase(Ease.OutQuad);
        treasureDescriptionText.text = "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //scaleAndShake.Animate();
        EventManager.Instance.ChooseTreasure(this.gameObject);
        treasureBG.gameObject.SetActive(false);
        treasureDescriptionText.text = "";
        treasureName.text = "";
        StartCoroutine(UIManager.Instance.AwardTreasure(this));
        
        // treasureName.gameObject.SetActive(false);
        // treasureDescriptionText.text = "";
        // transform.DOMove(chosenTreasurePosition, 1f).SetEase(Ease.OutQuad).OnComplete(()=>
        // {
        //     
        // });
    }

    public void HideSelf(GameObject t)
    {
        if (t != this.gameObject)
        {
            gameObject.SetActive(false);
        }
    }
}
