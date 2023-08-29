using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TurnIndicator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image background;
    public Image unitSprite;
    public UnitBehaviour unitBehaviour;
    public void OnPointerEnter(PointerEventData eventData)
    {
        unitBehaviour.Grow();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        unitBehaviour.Shrink();
    }

    public void SetBackgroundColor(Color32 color)
    {
        background.color = color;
    }
}
