using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionHandler : MonoBehaviour
{
    public static ActionHandler Instance;
    
    public Cell clickedCell;
    public Cell draggedCell;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SetClickedCell(Cell cell)
    {
        clickedCell = cell;
        
       // if (cell.unitBehaviour != null) cell.unitBehaviour.Grow();
    }

    public void SetDraggedCell(Cell cell)
    {
        if (cell == null || clickedCell == null) return;

        draggedCell = cell;

        // Check if clickedCell.unitBehaviour and unitData are null before accessing them
        if (clickedCell.unitBehaviour?.unitData == null) return;
        
        // Check if draggedCell.unitBehaviour and unitData are null before accessing them
        if (draggedCell != null && draggedCell.unitBehaviour?.unitData == null)
        {
            if (BoardManager.Instance.IsNeighbor(clickedCell, draggedCell))
            {
                ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.Swap, draggedCell);
            }
            else
            {
                ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.NoSwap, draggedCell);
            }

            return;
        }

        UnitBehaviour clickedUnit = GetUnitBasedOnTribe(clickedCell);

        // Exit if we couldn't find a clickedUnit
        if (clickedUnit == null) return;
        
        UnitBehaviour draggedUnit = GetUnitBasedOnTribe(draggedCell);

        // Exit if we couldn't find a draggedUnit
        if (draggedUnit == null) return;

        // Business logic
        if (clickedUnit.unitData.tribe == Unit.Tribe.Hero)
        {
            if (draggedUnit.unitData.tribe == Unit.Tribe.Hero)
            {
                if (BoardManager.Instance.IsHeroNeighbor(clickedCell, draggedCell))
                {
                    Debug.Log("Swap available");
                    ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.Swap, draggedCell);
                }
                else
                {
                    ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.NoSwap, draggedCell);
                }

                return;
            }

            if (BoardManager.Instance.CanAttack(clickedUnit, draggedUnit))
            {
                Debug.Log("Combat available");
                ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.Combat, draggedCell);
            }
            else
            {
                ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.NoCombat, draggedCell);
            }
        }

        else
        {
            if (draggedUnit.unitData.tribe == Unit.Tribe.Hero || !BoardManager.Instance.IsNeighbor(clickedCell, draggedCell))
            {
                ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.NoSwap, draggedCell);
            }
            else
            {
                ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.Swap, draggedCell);
            }
        }
        
        if (draggedUnit) draggedUnit.Grow();
    }

    public void RemoveDraggedCell()
    {
        draggedCell = null;
        ArrowLine.Instance.HideHoverIndicator();
        
    }
    
    private UnitBehaviour GetUnitBasedOnTribe(Cell cell)
    {
        if (cell.unitBehaviour.unitData.tribe == Unit.Tribe.Hero)
        {
            return BoardManager.Instance.GetHeroUnitBehaviourAtCoordinate(cell.column);
        }
        else
        {
            return BoardManager.Instance.GetUnitBehaviourAtCoordinate(cell.coordinates);
        }
    }

    public IEnumerator ResolveAction()
    {
        CursorAnimation.Instance.UnhighlightChain();
        ArrowLine.Instance.HideIndicators();
        
        
        if (!clickedCell || !draggedCell)
        {
            ClearUnits();
            yield break;
        }
        
        if (clickedCell.unitBehaviour == null)
        {
            ClearUnits();
            yield break;
        }
        
        if (!clickedCell) yield break;
        
        clickedCell.unitBehaviour.isDragging = false;
        
        // clicked a hero
        if (clickedCell.unitBehaviour.unitData.tribe == Unit.Tribe.Hero)
        {
            if (draggedCell.unitBehaviour == null) yield break;
            
            if (draggedCell.unitBehaviour.unitData.tribe == Unit.Tribe.Hero)
            {
                BoardManager.Instance.SwapHeroBlocks(clickedCell.column, draggedCell.column);
                yield break;
            }
            
            UnitBehaviour clickedUnit = GetUnitBasedOnTribe(clickedCell);

            // Exit if we couldn't find a clickedUnit
            if (clickedUnit == null) yield break;
        
            UnitBehaviour draggedUnit = GetUnitBasedOnTribe(draggedCell);

            // Exit if we couldn't find a draggedUnit
            if (draggedUnit == null) yield break;

            else if (BoardManager.Instance.CanAttack(clickedUnit, draggedUnit))
            {
                ResetUnitSize();
                // attack the unit
                yield return StartCoroutine(clickedCell.unitBehaviour.Attack(draggedUnit));
                BoardManager.Instance.CleanUpBoard();
            }
        }

        // clicked unit was enemy
        else
        {
            if (draggedCell.unitBehaviour == null && BoardManager.Instance.IsNeighbor(clickedCell, draggedCell))
            {
                BoardManager.Instance.SwapBlocks(clickedCell.coordinates, draggedCell.coordinates);
                ResetUnitSize();
                ClearUnits();
                yield break;
            }
            
            if (draggedCell.unitBehaviour.unitData.tribe != Unit.Tribe.Hero && BoardManager.Instance.IsNeighbor(clickedCell, draggedCell))
            {
                BoardManager.Instance.SwapBlocks(clickedCell.coordinates, draggedCell.coordinates);
                ResetUnitSize();
            }
        }

        
        ClearUnits();
    }

    private void ResetUnitSize()
    {
        // if (clickedCell)
        // {
        //     if (clickedCell.unitBehaviour) clickedCell.unitBehaviour.Shrink();
        // }
        //
        // if (draggedCell)
        // {
        //     if (draggedCell.unitBehaviour) draggedCell.unitBehaviour.Shrink();
        // }
    }

    private void ClearUnits()
    {
        clickedCell = null;
        draggedCell = null;
    }
}
