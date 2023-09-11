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
        
        if (cell.unitBehaviour != null) cell.unitBehaviour.Grow();
    }

    public void SetDraggedCell(Cell cell)
    {
        if (draggedCell)
        {
            if (draggedCell.unitBehaviour)
                draggedCell.unitBehaviour.Shrink();
        }
        // Early exit if the input cell is null
        if (cell == null || clickedCell == null) return;

        draggedCell = cell;

        // Check if clickedCell.unitBehaviour and unitData are null before accessing them
        if (clickedCell.unitBehaviour?.unitData == null) return;

        UnitBehaviour clickedUnit = GetUnitBasedOnTribe(clickedCell);

        // Exit if we couldn't find a clickedUnit
        if (clickedUnit == null) return;

        // Check if draggedCell.unitBehaviour and unitData are null before accessing them
        if (draggedCell.unitBehaviour?.unitData == null) return;

        UnitBehaviour draggedUnit = GetUnitBasedOnTribe(draggedCell);

        // Exit if we couldn't find a draggedUnit
        if (draggedUnit == null) return;

        // Business logic
        if (clickedUnit.unitData.tribe == Unit.Tribe.Hero)
        {
            if (draggedUnit.unitData.tribe == Unit.Tribe.Hero)
            {
                Debug.Log("Swap available");
                ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.Swap);
            }

            if (clickedUnit.combatTarget == draggedUnit)
            {
                Debug.Log("Combat available");
                ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.Combat);
            }
        }
        
        if (draggedUnit) draggedUnit.Grow();
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
        
        // clicked a hero
        if (clickedCell.unitBehaviour.unitData.tribe == Unit.Tribe.Hero)
        {
            if (draggedCell.unitBehaviour.unitData.tribe == Unit.Tribe.Hero)
            {
                // swap the units
            }

            else
            {
                ResetUnitSize();
                // attack the unit
                yield return StartCoroutine(clickedCell.unitBehaviour.Attack());
                BoardManager.Instance.CleanUpBoard();
            }
        }

        // clicked unit was enemy
        else
        {
            if (draggedCell.unitBehaviour == null)
            {
                BoardManager.Instance.SwapBlocks(clickedCell.coordinates, draggedCell.coordinates);
                ResetUnitSize();
                ClearUnits();
                yield break;
            }
            
            if (draggedCell.unitBehaviour.unitData.tribe != Unit.Tribe.Hero)
            {
                BoardManager.Instance.SwapBlocks(clickedCell.coordinates, draggedCell.coordinates);
                ResetUnitSize();
            }
        }

        
        ClearUnits();
    }

    private void ResetUnitSize()
    {
        if (clickedCell)
        {
            if (clickedCell.unitBehaviour) clickedCell.unitBehaviour.Shrink();
        }

        if (draggedCell)
        {
            if (draggedCell.unitBehaviour) draggedCell.unitBehaviour.Shrink();
        }
    }

    private void ClearUnits()
    {
        clickedCell = null;
        draggedCell = null;
    }
}