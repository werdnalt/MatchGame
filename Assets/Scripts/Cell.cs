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
        
        Debug.Log($"{coordinates.x}, {coordinates.y}");
        BoardManager.Instance.SetCellSelector(transform.position);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("clicking");
        unitBehaviour = BoardManager.Instance.GetUnitBehaviourAtCoordinate(coordinates);
        if (!unitBehaviour) return;
        
        unitBehaviour.AnimateJump();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CursorAnimation.Instance.isDragging) return;
        
        BoardManager.Instance.SetCellSelector(new Vector3(10000, 10000, 10000));
    }

    public void OnDrag(PointerEventData eventData)
    {
        CursorAnimation.Instance.ChangeColor(Color.cyan);
        CursorAnimation.Instance.StartDraggingFrom(eventData.position, coordinates);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CursorAnimation.Instance.StopDragging();
    }
}
