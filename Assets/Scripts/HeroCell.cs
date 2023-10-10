using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HeroCell: Cell, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
{
    private Vector3 originalScale;
    private bool scaleSet = false;
    
    [SerializeField]
    private float squashDuration = 0.1f;
    [SerializeField]
    private Vector3 squashedScale = new Vector3(1.2f, 0.8f, 1.2f); //Adjust these as per your object's original size and desired effect
    [SerializeField]
    private Vector3 stretchedScale = new Vector3(0.9f, 1.1f, 0.9f);
    [SerializeField]
    private float jiggleDuration = 0.9f;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        BoardManager.Instance.SetCellSelector(transform.position);

        if (!BoardManager.Instance.canMove) return;
        
        unitBehaviour = BoardManager.Instance.GetHeroUnitBehaviourAtCoordinate(column);

        if (eventData.dragging)
        {
            ActionHandler.Instance.SetDraggedCell(this);
        }
        if (!eventData.dragging)
        {
            ArrowLine.Instance.SetHoverIndicator(transform.position);
        }
        if (!unitBehaviour)
        {
            UIManager.Instance.HideUnitPanel();
            return;
        }
        
        ApplyJelloEffect(unitBehaviour);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ActionHandler.Instance.HideIndicators();
        UIManager.Instance.HideUnitPanel();
        //if (CursorAnimation.Instance.isDragging) return;
        
        //CursorAnimation.Instance.UnhighlightChain();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!unitBehaviour || unitBehaviour.isDragging) return;
        
        unitBehaviour.Jump();
        unitBehaviour.isDragging = true;
        ArrowLine.Instance.StartDrawingLine(unitBehaviour.transform.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartCoroutine(ActionHandler.Instance.ResolveAction());
        BoardManager.Instance.SetCellSelector(transform.position);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        unitBehaviour = BoardManager.Instance.GetHeroUnitBehaviourAtCoordinate(column);
        ActionHandler.Instance.SetClickedCell(this);
        
        AudioManager.Instance.PlayWithRandomPitch("click");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIManager.Instance.ShowUnitPanel(unitBehaviour);
        CursorAnimation.Instance.HighlightChain(unitBehaviour.combatTarget);
    }
    
    private void ApplyJelloEffect(UnitBehaviour unitBehaviour)
    {
        if (!unitBehaviour) return;
        
        GameObject go = unitBehaviour.gameObject;

        if (!scaleSet)
        {
            originalScale = go.transform.localScale;
            scaleSet = true;
        }
        
        Sequence jelloSequence = DOTween.Sequence();
        
        jelloSequence.Append(go.transform.DOScale(squashedScale, squashDuration)) //Squash down
            .Append(go.transform.DOScale(stretchedScale, squashDuration)) //Stretch out
            .Append(go.transform.DOScale(originalScale, jiggleDuration).SetEase(Ease.OutElastic)) //Return with a little jiggle using Elastic ease
            .OnKill(() => go.transform.localScale = originalScale); // Ensure it always returns to the original state when sequence is killed.

        jelloSequence.Play();
    }
}
