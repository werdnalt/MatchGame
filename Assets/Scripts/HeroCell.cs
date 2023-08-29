using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HeroCell: MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler, IDragHandler, IPointerUpHandler
{
    public int column;

    public UnitBehaviour unitBehaviour;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CursorAnimation.Instance.isDragging) return;
        
        BoardManager.Instance.SetCellSelector(transform.position);
        
        unitBehaviour = BoardManager.Instance.GetHeroUnitBehaviourAtCoordinate(column);
        if (!unitBehaviour) return;
        
        UIManager.Instance.ShowUnitPanel(unitBehaviour);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        unitBehaviour = BoardManager.Instance.GetHeroUnitBehaviourAtCoordinate(column);
        if (!unitBehaviour) return;
        
        AudioManager.Instance.PlayWithRandomPitch("click");
        UIManager.Instance.ShowUnitPanel(unitBehaviour);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CursorAnimation.Instance.isDragging) return;
    }

    public void OnDrag(PointerEventData eventData)
    {
        CursorAnimation.Instance.StartDraggingHeroFrom(eventData.position, column);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CursorAnimation.Instance.StopDragging();
    }
}
