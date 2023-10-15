using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TreasureUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{ 
    public Treasure treasure;

    [SerializeField] private TextMeshProUGUI durabilityText;
    [SerializeField] private Image treasureImage;

    private int _numUses;

    public void Setup(Treasure treasure)
    {
        this.treasure = treasure;
        treasureImage.sprite = treasure.treasureUISprite;
        durabilityText.text = treasure.effect.numUses.ToString();
        _numUses = treasure.effect.numUses;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.ShowToolTip(
            $"{treasure.name} \r\n " +
            $"{treasure.effect.effectDescription} \r\n " +
            $"Durability: {_numUses}");
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideToolTip();
    }

    public void Countdown()
    {
        _numUses--;
        durabilityText.text = _numUses.ToString();
        
        if (_numUses <= 0) RemoveTreasureUI();
    }

    private void RemoveTreasureUI()
    {
        Destroy(this.gameObject);
    }
}
