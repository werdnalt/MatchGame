using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
{
    public BoardManager.Coordinates coordinates;
    public int column;
    public UnitBehaviour unitBehaviour;

    private bool _isHolding;
    private bool _hasAnimatedHolding;

    private float _timeEnteredCell = Mathf.Infinity;
    private float _timeUntilChainShown = .5f;
    private float _infoPanelTimeHoverThreshold = .5f;
    
    private Vector3 originalScale;
    private bool scaleSet = false;
    private bool _hasShownPanel = false;
    
    private float _cachedZIndex;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlayWithRandomPitch("wood2");
        _timeEnteredCell = Time.time;
        unitBehaviour = BoardManager.Instance.GetUnitBehaviourAtCoordinate(coordinates);
        if (eventData.dragging)
        {
            ActionHandler.Instance.SetDraggedCell(this);
        }
        else
        {
            ArrowLine.Instance.SetHoverIndicator(transform.position);
        }
        if (!unitBehaviour)
        {
            
            return;
        }
        //UIManager.Instance.ShowUnitPanel(unitBehaviour);

        if (!unitBehaviour.healthUI.activeSelf) unitBehaviour.ShowAndUpdateHealth();
        
        unitBehaviour.ShowAttack();
        
        _cachedZIndex = unitBehaviour.transform.position.z;
        var cachedPos = unitBehaviour.transform.position;
        unitBehaviour.transform.position = new Vector3(cachedPos.x, cachedPos.y, -3);
        
        //ApplyJelloEffect(unitBehaviour);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Exit");
        ActionHandler.Instance.HideIndicators();
        _timeEnteredCell = Mathf.Infinity;
        
        UIManager.Instance.HideUnitPanel();
        // if (CursorAnimation.Instance.isDragging) return;
        //
        CursorAnimation.Instance.UnhighlightChain();
        _hasShownPanel = false;

        if (unitBehaviour)
        {
            var cachedPos = unitBehaviour.transform.position;
            unitBehaviour.transform.position = new Vector3(cachedPos.x, cachedPos.y, _cachedZIndex);
            if (unitBehaviour.coordinates.y != 0) unitBehaviour.HideHealth();
            if (unitBehaviour.coordinates.y != 0) unitBehaviour.HideAttack();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!unitBehaviour || unitBehaviour.isDragging || !BoardManager.Instance.canMove || unitBehaviour.isDead) return;

        AudioManager.Instance.Play("wood");
        unitBehaviour.Jump();
        unitBehaviour.isDragging = true;
        ArrowLine.Instance.StartDrawingLine(unitBehaviour.transform.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!unitBehaviour) return;
        
        unitBehaviour.isDragging = false;
        
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //CursorAnimation.Instance.CancelSelection();
        }

        StartCoroutine(ActionHandler.Instance.ResolveAction());
    }

    private void Update()
    {
        if (Time.time - _timeEnteredCell >= _timeUntilChainShown && !_hasShownPanel)
        {
            unitBehaviour = BoardManager.Instance.GetUnitBehaviourAtCoordinate(coordinates);
            if (!unitBehaviour)
            {
                return;
            }
        
            CursorAnimation.Instance.HighlightChain(unitBehaviour);
            _hasShownPanel = true;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!BoardManager.Instance.canMove) return;
        
        unitBehaviour = BoardManager.Instance.GetUnitBehaviourAtCoordinate(coordinates);

        if (!unitBehaviour) return;
        
        unitBehaviour.ApplyJelloEffect();
        
        ActionHandler.Instance.SetClickedCell(this);
        
        AudioManager.Instance.PlayWithRandomPitch("click");
    }
    

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!unitBehaviour) return;
        UIManager.Instance.ShowUnitPanel(unitBehaviour);
    }
}
