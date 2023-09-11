using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public BoardManager.Coordinates coordinates;
    public int column;
    public UnitBehaviour unitBehaviour;

    private bool _isHolding;
    private bool _hasAnimatedHolding;

    private float _timeEnteredCell = Mathf.Infinity;
    private float _timeUntilChainShown = .5f;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        unitBehaviour = BoardManager.Instance.GetUnitBehaviourAtCoordinate(coordinates);
        if (eventData.dragging) ActionHandler.Instance.SetDraggedCell(this);
        if (!unitBehaviour)
        {
            UIManager.Instance.HideUnitPanel();
            return;
        }
        
        UIManager.Instance.ShowUnitPanel(unitBehaviour);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        _timeEnteredCell = Mathf.Infinity;
        // if (CursorAnimation.Instance.isDragging) return;
        //
        // CursorAnimation.Instance.UnhighlightChain();
    }

    public void OnDrag(PointerEventData eventData)
    {
        //CursorAnimation.Instance.StartDraggingFrom(eventData.position, coordinates);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //CursorAnimation.Instance.CancelSelection();
        }

        StartCoroutine(ActionHandler.Instance.ResolveAction());
    }

    private void Update()
    {
        if (Time.time - _timeEnteredCell >= _timeUntilChainShown)
        {
            unitBehaviour = BoardManager.Instance.GetUnitBehaviourAtCoordinate(coordinates);
            if (!unitBehaviour)
            {
                return;
            }
        
            //CursorAnimation.Instance.HighlightChain(unitBehaviour);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        unitBehaviour = BoardManager.Instance.GetUnitBehaviourAtCoordinate(coordinates);
        ActionHandler.Instance.SetClickedCell(this);
        
        AudioManager.Instance.PlayWithRandomPitch("click");
    }
}
