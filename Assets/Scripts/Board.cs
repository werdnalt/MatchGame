using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board
{
    private BoardPosition[][] _boardPositions;
    private BoardPosition[] _heroPositions;
    private int _numColumns;
    private int _numRows;

    public UnitBehaviour[] FrontRowEnemies
    {
        get
        {
            return _boardPositions
                .Select(row => row.First())
                .Where(boardPosition => boardPosition.unit != null)
                .Select(position => position.unit).ToArray();
        }
    }

    public UnitBehaviour[] Heroes { get; }

    public Board(int numColumns, int numRows)
    {
        _numColumns = numColumns;
        _numRows = numRows;
        Heroes = new UnitBehaviour[numColumns];

        _boardPositions = new BoardPosition[_numColumns][];
        _heroPositions = new BoardPosition[_numColumns];

        for (int i = 0; i < numColumns; i++) // changed from numRows to _fullColumnCount
        {
            BoardPosition[] column = new BoardPosition[numRows];
            _boardPositions[i] = column;
            for (int j = 0; j < _numRows; j++) 
            {
                BoardManager.Coordinates boardCoordinates = new BoardManager.Coordinates(i, j);
                Vector3 worldPosition = WorldPositionForBoardCoordinate(boardCoordinates);
                column[j] = new BoardPosition(boardCoordinates, worldPosition);
            }
        }

        for (int i = 0; i < numColumns; i++)
        {
            Vector3 worldPosition = WorldPositionForHeroPositionIndex(i);
            BoardManager.Coordinates coordinates = new BoardManager.Coordinates(i, 0);
            _heroPositions[i] = new BoardPosition(coordinates, worldPosition);
        }
    }

    private Vector3 WorldPositionForBoardCoordinate(BoardManager.Coordinates boardCoordinates)
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

        return new Vector3(xPos, yPos);
    }

    private Vector3 WorldPositionForHeroPositionIndex(int index)
    {
        // Calculate the total width and height of the board
        float boardWidth = _numColumns; // Assuming each board position has a width of 1 unit
        float boardHeight = _numRows; // Assuming each board position has a height of 1 unit

        // Calculate the starting position for the board to be centered on the screen
        float startX = -boardWidth / 2 + 0.5f; // +0.5f since we assume each board position has a width of 1 unit and we want to start from its center
        float startY = -boardHeight / 2 - 1f; // Add offset to move heros lower than the board

        float xPos = startX + index;

        return new Vector3(xPos, startY);
    }
    
    public GameObject GetUnitGameObject(BoardManager.Coordinates coordinates)
    {
        UnitBehaviour unit = GetUnitBehaviour(coordinates);
        if (unit)
        {
            return unit.gameObject;
        }
        return null;
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
        if (unitBehaviour != null)
        {
            unitBehaviour.transform.position = boardPosition.worldSpacePosition;
        }

        _boardPositions[coordinates.x][coordinates.y] = boardPosition;
    }

    public void SwapBlocks(BoardManager.Coordinates leftBlockCoords, BoardManager.Coordinates rightBlockCoords)
    {
        // Cache original blocks
        var originalLeftBlock = GetUnitBehaviour(leftBlockCoords);
        var originalRightBlock = GetUnitBehaviour(rightBlockCoords);
    
        // Now you can swap the blocks
        SetUnitBehaviour(leftBlockCoords, originalRightBlock);
        SetUnitBehaviour(rightBlockCoords, originalLeftBlock);
    }
    
    public bool AreColumnsFull()
    {
        for (int column = 0; column < _numColumns; column++)
        {
            for (var row = 0; row < _numRows; row++)
            {
                if (_boardPositions[column][row].unit == null)
                {
                    return false;
                }
            }
        }
        
        return true;
    }

    /// <summary>
    /// Finds the coordinates of the first available position in a random column
    /// </summary>
    /// <returns>
    /// The coordinate of the position. If board is full, returns null
    /// </returns>
    public BoardManager.Coordinates? FindRandomColumn()
    {
        List<int> availableColumnIndices = new List<int>();

        // find any columns that have at least one available cell
        for (int column = 0; column < _numColumns; column++)
        {
            for (int row = 0; row < +_numRows; row++)
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
        BoardManager.Coordinates? cellCoordinates = FindFirstAvailablePositionInColumn(chosenColumnIndex);

        return cellCoordinates;
    }

    private BoardManager.Coordinates? FindFirstAvailablePositionInColumn(int columnIndex)
    {
        for (int row = 0; row < _numRows; row++)
        {
            if (GetUnitBehaviour(new BoardManager.Coordinates(columnIndex, row)) == null)
            {
                return new BoardManager.Coordinates(columnIndex, row);
            }
        }

        return null;
    }

    public Vector3 GetWorldSpacePositionForBoardCoordinates(BoardManager.Coordinates coordinates)
    {
        if (coordinates.x > _numColumns || coordinates.y > _numRows)
        {
            Debug.LogAssertion("Attempting to access coordinate outside of the board space");
            return Vector3.zero;
        }

        return _boardPositions[coordinates.x][coordinates.y].worldSpacePosition;

    }

    public Vector3? GetWorldSpacePositionForUnitBehaviour(UnitBehaviour unit)
    {
        BoardManager.Coordinates? boardCoordinates = FindUnitBehaviour(unit);
        if (boardCoordinates != null)
        {
            return GetWorldSpacePositionForBoardCoordinates(boardCoordinates.Value);
        }
        return null;
    }
        

    public Vector3? SetHero(UnitBehaviour heroUnit)
    {
        for (int i = 0; i < _heroPositions.Length; i++)
        {
            if (_heroPositions[i].unit == null)
            {
                _heroPositions[i].unit = heroUnit;
                return _heroPositions[i].worldSpacePosition;
            }
        }

        Debug.LogAssertion("Attempting to add a hero, but there isn't room");
        return null;
    }

    public BoardManager.Coordinates? FindUnitBehaviour(UnitBehaviour unit)
    {
        for (int i = 0; i < _boardPositions.Length; i++)
        {
            for (int j = 0; j < _boardPositions[i].Length; j++)
            {
                if (_boardPositions[i][j].unit == unit)
                {
                    return new BoardManager.Coordinates(i, j);
                }
            }
        }

        return null; // return null if the object was not found
    }

    public void RemoveUnitFromBoard(UnitBehaviour unit)
    {
        var unitCoords = FindUnitBehaviour(unit);
        if (unitCoords == null) return;
        _boardPositions[unitCoords.Value.x][unitCoords.Value.y].unit = null;
    }

}
