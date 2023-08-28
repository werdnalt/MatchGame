using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler, IDragHandler, IPointerUpHandler
{
    public BoardManager.Coordinates coordinates;
    public UnitBehaviour unitBehaviour;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CursorAnimation.Instance.isDragging) return;
        
        BoardManager.Instance.SetCellSelector(transform.position);
        
        unitBehaviour = BoardManager.Instance.GetUnitBehaviourAtCoordinate(coordinates);
        if (!unitBehaviour) return;
        
        UIManager.Instance.ShowUnitPanel(unitBehaviour);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        unitBehaviour = BoardManager.Instance.GetUnitBehaviourAtCoordinate(coordinates);
        if (!unitBehaviour) return;
        
        AudioManager.Instance.PlayWithRandomPitch("click");
        UIManager.Instance.ShowUnitPanel(unitBehaviour);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CursorAnimation.Instance.isDragging) return;
        
        BoardManager.Instance.SetCellSelector(new Vector3(10000, 10000, 10000));
    }

    public void OnDrag(PointerEventData eventData)
    {
        CursorAnimation.Instance.StartDraggingFrom(eventData.position, coordinates);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CursorAnimation.Instance.StopDragging();
    }
}
