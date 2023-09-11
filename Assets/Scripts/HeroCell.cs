using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HeroCell: Cell, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        //if (CursorAnimation.Instance.isDragging) return;
        
        BoardManager.Instance.SetCellSelector(transform.position);
        
        unitBehaviour = BoardManager.Instance.GetHeroUnitBehaviourAtCoordinate(column);
        if (!unitBehaviour) return;
        
        //CursorAnimation.Instance.HighlightChain(unitBehaviour.combatTarget);
        UIManager.Instance.ShowUnitPanel(unitBehaviour);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //if (CursorAnimation.Instance.isDragging) return;
        
        //CursorAnimation.Instance.UnhighlightChain();
    }

    public void OnDrag(PointerEventData eventData)
    {
        //CursorAnimation.Instance.StartDraggingHeroFrom(eventData.position, column);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartCoroutine(ActionHandler.Instance.ResolveAction());
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        unitBehaviour = BoardManager.Instance.GetHeroUnitBehaviourAtCoordinate(column);
        ActionHandler.Instance.SetClickedCell(this);
        
        AudioManager.Instance.PlayWithRandomPitch("click");
    }
}
