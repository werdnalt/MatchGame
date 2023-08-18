using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    private List<List<GameObject>> _board;
    private List<List<GameObject>> _unitPositions;
    private int _numColumns;
    private int _numRows;
    private int _offsetRows;

    public UnitBehaviour[] FrontRowEnemyPositions { get; }
    public UnitBehaviour[] HeroPositions { get; }

    public Board(int numColumns, int numRows)
    {
        _board = new List<List<GameObject>>();
        _unitPositions = new List<List<GameObject>>();
        _numColumns = numColumns;
        _numRows = numRows;
        FrontRowEnemyPositions = new UnitBehaviour[numColumns];
        HeroPositions = new UnitBehaviour[numColumns];
        _offsetRows = _numRows + 2; // this accounts for hero and blank row
        
        for (int i = 0; i < numColumns; i++) // changed from numRows to _fullColumnCount
        {
            _board.Add(new List<GameObject>());
            _unitPositions.Add(new List<GameObject>());
            for (int j = 0; j < _numRows + 2; j++) 
            {
                _board[i].Add(null);         // or initialize with a default value
                _unitPositions[i].Add(null); // or initialize with a default value
            }
        }
    }
    
    public GameObject GetUnitGameObject(BoardManager.Coordinates coordinates)
    {
        return _board[coordinates.x][coordinates.y] ? _board[coordinates.x][coordinates.y] : null;
    }
    
    public UnitBehaviour GetUnitBehaviour(BoardManager.Coordinates coordinates)
    {
        return _board[coordinates.x][coordinates.y] ? _board[coordinates.x][coordinates.y].GetComponent<UnitBehaviour>() : null;
    }

    public void SetUnitBehaviour(BoardManager.Coordinates coordinates, GameObject unitBehaviourObject)
    {
        _board[coordinates.x][coordinates.y] = unitBehaviourObject;

        // unit is in the front/attacking row
        if (coordinates.y == 2)
        {
            FrontRowEnemyPositions[coordinates.x] = unitBehaviourObject.GetComponent<UnitBehaviour>();
        }
        
        if (coordinates.y == 0)
        {
            HeroPositions[coordinates.x] = unitBehaviourObject.GetComponent<UnitBehaviour>();
        }
    }

    public void SetUnitPositions(BoardManager.Coordinates coordinates, GameObject positionAnchor)
    {
        _unitPositions[coordinates.x][coordinates.y] = positionAnchor;
    }
    
    public void SetCoordinates(BoardManager.Coordinates coordinates)
    {
        var blockGameObject = _board[coordinates.x][coordinates.y];
        if (blockGameObject && blockGameObject.GetComponent<UnitBehaviour>())
        {
            blockGameObject.GetComponent<UnitBehaviour>().coordinates = coordinates;
        }
    }

    public void SwapBlocks(BoardManager.Coordinates leftBlockCoords, BoardManager.Coordinates rightBlockCoords)
    {
        // Cache original blocks
        var originalLeftBlock = GetUnitGameObject(leftBlockCoords);
        var originalRightBlock = GetUnitGameObject(rightBlockCoords);
    
        // Now you can swap the blocks
        SetUnitBehaviour(leftBlockCoords, originalRightBlock);
        SetUnitBehaviour(rightBlockCoords, originalLeftBlock);
    }
    
    public bool AreColumnsFull()
    {
        var full = true;
        

        for (int column = 0; column < _numColumns; column++)
        {
            for (var row = 2; row < _offsetRows; row++)
            {
                if (_board[column][row].GetComponent<UnitBehaviour>().unit != null) continue;
                full = false;
                break;
            }
        }
        
        return full;
    }

    public BoardManager.Coordinates FindRandomColumn()
    {
        List<int> availableColumnIndices = new List<int>();

        // find any columns that have at least one available cell
        for (int column = 0; column < _numColumns; column++)
        {
            for (int row = 2; row < _offsetRows; row++)
            {
                if (GetUnitBehaviour(new BoardManager.Coordinates(column, row)) == null)
                {
                    availableColumnIndices.Add(column);
                    break;  // Exit the inner loop once we find an available cell in the column.
                }
            }
        }

        // If there are no available columns, handle this case.
        if (availableColumnIndices.Count == 0)
        {
            // Depending on your requirements, return a default value or handle differently.
            return new BoardManager.Coordinates(-1, -1);
        }

        // pick a random column from the available columns
        int chosenColumnIndex = availableColumnIndices[Random.Range(0, availableColumnIndices.Count)];

        // Find the first available cell in the chosen column.
        BoardManager.Coordinates cellCoordinates = FindFirstAvailableCellInColumn(chosenColumnIndex);

        return cellCoordinates;
    }

    private BoardManager.Coordinates FindFirstAvailableCellInColumn(int columnIndex)
    {
        for (int row = 2; row < _offsetRows; row++)
        {
            if (GetUnitBehaviour(new BoardManager.Coordinates(columnIndex, row)) == null)
            {
                return new BoardManager.Coordinates(columnIndex, row);
            }
        }

        return new BoardManager.Coordinates(-1, -1);
    }


    public Vector3 GetUnitPosition(BoardManager.Coordinates coordinates)
    {
        if (_unitPositions[coordinates.x][coordinates.y])
        {
            return _unitPositions[coordinates.x][coordinates.y].transform.position;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public BoardManager.Coordinates SetHero(GameObject heroObject)
    {
        var heroRow = 0;
        BoardManager.Coordinates coordinates = new BoardManager.Coordinates(-1, -1);
        
        // find available block for hero if possible
        for (var potentialColumnIndex = 0; potentialColumnIndex < _numColumns; potentialColumnIndex++)
        {
            var heroCoords = new BoardManager.Coordinates(potentialColumnIndex, heroRow);
            var occupyingUnit = GetUnitBehaviour(heroCoords);
            
            if (occupyingUnit) continue;
            
            // assign hero data to that block
            SetUnitBehaviour(heroCoords, heroObject);
            coordinates = heroCoords;
            break;
        }
        
        return coordinates;
    }

}
