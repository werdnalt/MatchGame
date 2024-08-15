using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IPointerUpHandler
{
    public Coordinates Coordinates;
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
        
        
        if (!_unitBehaviour)
        {
            
            return;
        }
        
        UIManager.Instance.ShowUnitPanel(_unitBehaviour);
        
        if (!_unitBehaviour.healthUI.activeSelf) _unitBehaviour.ShowAndUpdateHealth();
        if (!_unitBehaviour.attackUI.activeSelf) _unitBehaviour.ShowAttack();

        _cachedZIndex = _unitBehaviour.transform.position.z;
        var cachedPos = _unitBehaviour.transform.position;
        _unitBehaviour.transform.position = new Vector3(cachedPos.x, cachedPos.y, -3);
        
        _unitBehaviour.ApplyJelloEffect();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        ActionHandler.Instance.HideIndicators();
        UIManager.Instance.HideUnitPanel();
        CursorAnimation.Instance.UnhighlightChain();

        if (!_unitBehaviour) return;
        
        var cachedPos = _unitBehaviour.transform.position;
        _unitBehaviour.transform.position = new Vector3(cachedPos.x, cachedPos.y, _cachedZIndex);
        
        // Don't hide health and attack if unit is sitting in enemy front row
        if (Coordinates.row == Timings.EnemyRow) return;
        _unitBehaviour.HideHealth();
        _unitBehaviour.HideAttack();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_unitBehaviour || _unitBehaviour.isDragging || !GamePlayDirector.Instance.playerActionPermitted || _unitBehaviour.isDead) return;

        AudioManager.Instance.Play("wood");
        _unitBehaviour.Jump();
        _unitBehaviour.Drag(true);
        ArrowLine.Instance.StartDrawingLine(_unitBehaviour.transform.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_unitBehaviour) return;
        
        _unitBehaviour.isDragging = false;
        
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //CursorAnimation.Instance.CancelSelection();
        }

        StartCoroutine(ActionHandler.Instance.ResolveAction());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!GamePlayDirector.Instance.playerActionPermitted) return;
        
        _unitBehaviour = BoardManager.Instance.GetUnitBehaviour(Coordinates);

        if (!_unitBehaviour) return;
        
        _unitBehaviour.ApplyJelloEffect();
        ActionHandler.Instance.SetClickedCell(this);
        AudioManager.Instance.PlayWithRandomPitch("click");
    }
}
