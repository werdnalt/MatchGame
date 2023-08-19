using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    private BoardPosition[][] _boardPositions;
    private int _numColumns;
    private int _numRows;

    public UnitBehaviour[] FrontRowEnemies { get; }
    public UnitBehaviour[] Heroes { get; }

    public Board(int numColumns, int numRows)
    {
        _numColumns = numColumns;
        _numRows = numRows;
        FrontRowEnemies = new UnitBehaviour[numColumns];
        Heroes = new UnitBehaviour[numColumns];

        _boardPositions = new BoardPosition[_numColumns][];

        for (int i = 0; i < numColumns; i++) // changed from numRows to _fullColumnCount
        {
            BoardPosition[] column = new BoardPosition[numRows];
            _boardPositions[i] = column;
            for (int j = 0; j < _numRows; j++) 
            {
                Vector2 boardCoordinates = new Vector2(i, j);
                Vector3 worldPosition = WorldPositionForBoardCoordinate(boardCoordinates);
                column[j] = new BoardPosition(boardCoordinates, worldPosition);
            }
        }
    }

    private Vector3 WorldPositionForBoardCoordinate(Vector2 boardCoordinates)
    {
        // Calculate the total width and height of the board
        float boardWidth = _numColumns; // Assuming each board position has a width of 1 unit
        float boardHeight = _numRows; // Assuming each board position has a height of 1 unit

        // Calculate the starting position for the board to be centered on the screen
        float startX = -boardWidth / 2 + 0.5f; // +0.5f since we assume each board position has a width of 1 unit and we want to start from its center
        float startY = -boardHeight / 2 + 0.5f; // +0.5f for the same reason as above


        // Calculate the position for each board position based on the start position
        float xPos = startX + boardCoordinates.x;
        float yPos = startY + boardCoordinates.y;

        return new Vector3(xPos, yPos, 0);
    }
    
    public GameObject GetUnitGameObject(BoardManager.Coordinates coordinates)
    {
        return GetUnitBehaviour(coordinates).gameObject;
    }
    
    public UnitBehaviour GetUnitBehaviour(BoardManager.Coordinates coordinates)
    {
        if (coordinates.x > _numColumns || coordinates.y > _numRows)
        {
            Debug.LogAssertion("Attempting to access coordinate outside of the board space");
            return null;
        }

        return  _boardPositions[coordinates.x][coordinates.y].unit;
    }

    public UnitBehaviour[] GetAllUnits()
    {
        List<UnitBehaviour> allUnits = new List<UnitBehaviour>();
        for(int column = 0; column < _boardPositions.Length; column++)
        {
            for (int row = 0; row < _boardPositions[column].Length; row++)
            {
                BoardPosition position = _boardPositions[column][row];
                if (position.unit != null)
                {
                    allUnits.Add(position.unit);
                }
            }
        }

        return allUnits.ToArray();
    }

    public void SetUnitBehaviour(BoardManager.Coordinates coordinates, UnitBehaviour unitBehaviour)
    {
        BoardPosition boardPosition = _boardPositions[coordinates.x][coordinates.y];
        boardPosition.unit = unitBehaviour;
        
        // TODO: Should this be automatically moved to the position?
        unitBehaviour.transform.position = boardPosition.worldSpacePosition;

        _boardPositions[coordinates.x][coordinates.y] = boardPosition;
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
                if (_units[column][row].GetComponent<UnitBehaviour>().unit != null) continue;
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

    public BoardManager.Coordinates? FindBlock(GameObject block)
    {
        for (int i = 0; i < _units.Count; i++)
        {
            for (int j = 0; j < _units[i].Count; j++)
            {
                if (_units[i][j] == block)
                {
                    return new BoardManager.Coordinates(i, j);
                }
            }
        }

        return null; // return null if the object was not found
    }

    public void RemoveBlock(GameObject blockToRemove)
    {
        var blockCoords = FindBlock(blockToRemove);
        if (blockCoords == null) return;
        _units[blockCoords.Value.x][blockCoords.Value.y] = null;
    }

}
