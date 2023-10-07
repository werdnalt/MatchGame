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
    
    [SerializeField]
    private float squashDuration = 0.1f;
    [SerializeField]
    private Vector3 squashedScale = new Vector3(1.2f, 0.8f, 1.2f); //Adjust these as per your object's original size and desired effect
    [SerializeField]
    private Vector3 stretchedScale = new Vector3(0.9f, 1.1f, 0.9f);
    [SerializeField]
    private float jiggleDuration = 0.9f;

    private float _cachedZIndex;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
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

        _cachedZIndex = unitBehaviour.transform.position.z;
        var cachedPos = unitBehaviour.transform.position;
        unitBehaviour.transform.position = new Vector3(cachedPos.x, cachedPos.y, -3);
        
        ApplyJelloEffect(unitBehaviour);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        ArrowLine.Instance.HideHoverIndicator();
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
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!unitBehaviour || unitBehaviour.isDragging || !BoardManager.Instance.canMove) return;
        
        unitBehaviour.Jump();
        unitBehaviour.isDragging = true;
        ArrowLine.Instance.StartDrawingLine(unitBehaviour.transform.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (unitBehaviour) unitBehaviour.isDragging = false;
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
        ApplyJelloEffect(unitBehaviour);
        ActionHandler.Instance.SetClickedCell(this);
        
        AudioManager.Instance.PlayWithRandomPitch("click");
    }
    
    private void ApplyJelloEffect(UnitBehaviour unitBehaviour)
    {
        if (!unitBehaviour || DOTween.IsTweening(unitBehaviour.gameObject)) return;
        
        var go = unitBehaviour.gameObject;

        go.transform.DOKill();

        if (!scaleSet)
        {
            originalScale = new Vector3(1, 1, 1);
            scaleSet = true;
        }
        
        Sequence jelloSequence = DOTween.Sequence();
        
        jelloSequence.Append(go.transform.DOScale(squashedScale, squashDuration)) //Squash down
            .Append(go.transform.DOScale(stretchedScale, squashDuration)) //Stretch out
            .Append(go.transform.DOScale(originalScale, jiggleDuration).SetEase(Ease.OutElastic)) //Return with a little jiggle using Elastic ease
            .OnKill(() => go.transform.localScale = originalScale); // Ensure it always returns to the original state when sequence is killed.

        jelloSequence.Play();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIManager.Instance.ShowUnitPanel(unitBehaviour);
    }
}
