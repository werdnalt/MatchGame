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

        Debug.Log("Set dragged cell");
        draggedCell = cell;

        // Check if clickedCell.unitBehaviour and unitData are null before accessing them
        if (clickedCell.UnitBehaviour?.unitData == null) return;
        
        // Check if draggedCell.unitBehaviour and unitData are null before accessing them
        if (draggedCell != null && draggedCell.UnitBehaviour?.unitData == null)
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

        var clickedUnit = clickedCell.UnitBehaviour;

        // Exit if we couldn't find a clickedUnit
        if (clickedUnit == null) return;

        var draggedUnit = draggedCell.UnitBehaviour;

        // Exit if we couldn't find a draggedUnit
        if (draggedUnit == null) return;

        // Business logic
        if (clickedUnit is HeroUnitBehaviour)
        {
            if (draggedUnit is HeroUnitBehaviour)
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

            // Dragged unit is an enemy
            if (UnitManager.CanAttack(clickedUnit, draggedUnit))
            {
                ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.Combat, draggedCell);
                // if (clickedUnit.attack > draggedUnit.currentHp) draggedUnit.skull.SetActive(true);
            }
            else
            {
                ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.NoCombat, draggedCell);
            }
        }

        else
        {
            if (draggedUnit is HeroUnitBehaviour || !BoardManager.Instance.IsNeighbor(clickedCell, draggedCell))
            {
                ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.NoSwap, draggedCell);
            }
            else
            {
                ArrowLine.Instance.SetIndicator(ArrowLine.IndicatorType.Swap, draggedCell);
            }
        }
    }
    
    public void HideIndicators()
    {
        ArrowLine.Instance.HideIndicators();
    }

    public IEnumerator ResolveAction()
    {
        if (clickedCell) clickedCell.UnitBehaviour.isDragging = false;
        
        CursorAnimation.Instance.UnhighlightChain();
        ArrowLine.Instance.HideIndicators();
        
        if (!GamePlayDirector.Instance.PlayerActionAllowed) yield break;
        
        if (!clickedCell || !draggedCell)
        {
            ClearSelectedCells();
            yield break;
        }
        
        if (clickedCell.UnitBehaviour == null)
        {
            ClearSelectedCells();
            yield break;
        }
        
        if (!clickedCell) yield break;
        
        clickedCell.UnitBehaviour.isDragging = false;
        
        // clicked a hero
        if (clickedCell.UnitBehaviour is HeroUnitBehaviour)
        {
            if (draggedCell.UnitBehaviour == null) yield break;
            
            if (draggedCell.UnitBehaviour is HeroUnitBehaviour)
            {
                ArrowLine.Instance.SetHoverIndicator(draggedCell.transform.position);
                yield return StartCoroutine(BoardManager.Instance.SwapUnits(clickedCell.Coordinates, draggedCell.Coordinates));
                yield break;
            }
            
            var clickedUnit = BoardManager.Instance.GetUnitBehaviour(clickedCell.Coordinates);

            // Exit if we couldn't find a clickedUnit
            if (clickedUnit == null) yield break;
        
            var draggedUnit = BoardManager.Instance.GetUnitBehaviour(draggedCell.Coordinates);

            // Exit if we couldn't find a draggedUnit
            if (draggedUnit == null) yield break;

            if (UnitManager.CanAttack(clickedUnit, draggedUnit))
            {
                yield return UnitManager.Instance.ExecuteAttack(clickedUnit, draggedUnit);
            }
        }
        
        // clicked unit was enemy
        else
        {
            if (draggedCell.UnitBehaviour == null && !BoardManager.Instance.IsNeighbor(clickedCell, draggedCell))
            {
                ArrowLine.Instance.SetHoverIndicator(draggedCell.transform.position);
                ClearSelectedCells();
                yield break;
            }
            
            if (draggedCell.UnitBehaviour == null && BoardManager.Instance.IsNeighbor(clickedCell, draggedCell))
            {
                ArrowLine.Instance.SetHoverIndicator(draggedCell.transform.position);
                StartCoroutine(BoardManager.Instance.SwapUnits(clickedCell.Coordinates, draggedCell.Coordinates));
                ClearSelectedCells();
                yield break;
            }
            
            if (draggedCell.UnitBehaviour.unitData.tribe != Unit.Tribe.Hero && BoardManager.Instance.IsNeighbor(clickedCell, draggedCell))
            {
                ArrowLine.Instance.SetHoverIndicator(draggedCell.transform.position);
                StartCoroutine(BoardManager.Instance.SwapUnits(clickedCell.Coordinates, draggedCell.Coordinates));
            }
        }

        
        ClearSelectedCells();
    }
    
    private void ClearSelectedCells()
    {
        clickedCell = null;
        draggedCell = null;
    }
}
