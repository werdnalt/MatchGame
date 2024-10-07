using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IPointerUpHandler
{
    private Coordinates _coordinates;
    public Coordinates Coordinates
    {
        set;
        get;
    }
    public Vector3 boardPosition;
    public UnitBehaviour UnitBehaviour
    {
        get => _unitBehaviour;
        set => _unitBehaviour = value;
    }

    private UnitBehaviour _unitBehaviour;
    private float _cachedZIndex;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlayWithRandomPitch("wood2");
        _unitBehaviour = BoardManager.Instance.GetUnitBehaviour(Coordinates);
        
        if (eventData.dragging)
        {
            ActionHandler.Instance.SetDraggedCell(this);
        }
        else
        {
            ArrowLine.Instance.SetHoverIndicator(transform.position);
        }
        
        if (!_unitBehaviour) return;
        
        UIManager.Instance.ShowUnitPanel(_unitBehaviour);
        _unitBehaviour.Grow();
        _unitBehaviour.ShowAndUpdateHealth();
        _unitBehaviour.BringSortingToFront();
        
        _unitBehaviour.ApplyJelloEffect();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        ActionHandler.Instance.HideIndicators();
        UIManager.Instance.HideUnitPanel();
        CursorAnimation.Instance.UnhighlightChain();
        ArrowLine.Instance.HideHoverIndicator();

        if (!_unitBehaviour) return;
        
        _unitBehaviour.Shrink();
        _unitBehaviour.ResetSortingOrder();
        var cachedPos = _unitBehaviour.transform.position;
        _unitBehaviour.transform.position = new Vector3(cachedPos.x, cachedPos.y, _cachedZIndex);
        
        // Don't hide health and attack if unit is sitting in enemy front row
        if (Coordinates.row == Timings.EnemyRow) return;
        _unitBehaviour.HideHealth();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_unitBehaviour || _unitBehaviour.isDragging || !GamePlayDirector.Instance.PlayerActionAllowed || _unitBehaviour.isDead || _unitBehaviour.IsStuck) return;

        ActionHandler.Instance.SetClickedCell(this);
        AudioManager.Instance.Play("wood");
        //_unitBehaviour.Jump();
        _unitBehaviour.Drag(true);
        ArrowLine.Instance.StartDrawingLine(_unitBehaviour.transform.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartCoroutine(ActionHandler.Instance.ResolveAction());
        
        if (!_unitBehaviour) return;
        
        _unitBehaviour.isDragging = false;
        
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //CursorAnimation.Instance.CancelSelection();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!GamePlayDirector.Instance.PlayerActionAllowed) return;
        
        _unitBehaviour = BoardManager.Instance.GetUnitBehaviour(Coordinates);

        if (!_unitBehaviour) return;
        
        _unitBehaviour.ApplyJelloEffect();
        ActionHandler.Instance.SetClickedCell(this);
        AudioManager.Instance.PlayWithRandomPitch("click");
    }
}
