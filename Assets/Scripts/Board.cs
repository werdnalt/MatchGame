using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class Board
{
    public float blockSwapTime;
    public BoardPosition[][] boardPositions;
    public BoardPosition[] heroPositions;
    private int _numColumns;
    private int _numRows;

    public UnitBehaviour[] FrontRowEnemies
    {
        get
        {
            return boardPositions
                .Select(row => row.First())
                .Select(position => position.unit).ToArray();
        }
    }

    public UnitBehaviour[] Heroes { get; }

    public Board(int numColumns, int numRows, Vector2 prefabSize)
    {
        blockSwapTime = .2f;
        _numColumns = numColumns;
        _numRows = numRows;
        Heroes = new UnitBehaviour[numColumns];

        boardPositions = new BoardPosition[_numColumns][];
        heroPositions = new BoardPosition[_numColumns];

        for (int i = 0; i < numColumns; i++) // changed from numRows to _fullColumnCount
        {
            BoardPosition[] column = new BoardPosition[numRows];
            boardPositions[i] = column;
            for (int j = 0; j < _numRows; j++) 
            {
                BoardManager.Coordinates boardCoordinates = new BoardManager.Coordinates(i, j);
                Vector3 worldPosition = WorldPositionForBoardCoordinate(boardCoordinates, prefabSize);
                column[j] = new BoardPosition(boardCoordinates, worldPosition);
            }
        }

        for (int i = 0; i < numColumns; i++)
        {
            Vector3 worldPosition = WorldPositionForHeroPositionIndex(i, prefabSize); // Assuming hero size is the same, or you can adjust as needed
            BoardManager.Coordinates coordinates = new BoardManager.Coordinates(i, 0);
            heroPositions[i] = new BoardPosition(coordinates, worldPosition);
        }
    }

    private Vector3 WorldPositionForBoardCoordinate(BoardManager.Coordinates boardCoordinates, Vector2 prefabSize)
    {
        var yOffset = .5f;
        // Calculate the total width and height of the board based on prefab size
        float boardWidth = _numColumns * prefabSize.x; 
        float boardHeight = (_numRows - 1) * prefabSize.x;

        // Calculate the starting position for the board to be centered on the screen
        float startX = -boardWidth / 2 + prefabSize.x / 2; 
        float startY = (-boardHeight / 2) + prefabSize.x / 2 + boardCoordinates.y * prefabSize.y + yOffset;  

        float xPos = startX + boardCoordinates.x * prefabSize.x;
        return new Vector3(xPos, startY, 0f); // Assuming Z value is 0, adjust if needed
    }

    private Vector3 WorldPositionForHeroPositionIndex(int index, Vector2 prefabSize)
    {
        // Calculate the total width and height of the board based on prefab size
        float boardWidth = _numColumns * prefabSize.x; 
        float boardHeight = (_numRows - 1) * prefabSize.x;

        // Calculate the starting position for the board to be centered on the screen
        float startX = -boardWidth / 2 + prefabSize.x / 2; 

        // Since heroes are below the board, place them half a unit (in terms of prefab size) below the bottom-most row of the board.
        float startY = (-boardHeight / 2) - prefabSize.y * .9f;  

        float xPos = startX + index * prefabSize.x;
        return new Vector3(xPos, startY, 0f); // Assuming Z value is 0, adjust if needed
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
        if (coordinates.x > _numColumns || coordinates.y > _numRows || coordinates.Equals(new BoardManager.Coordinates(-1, -1)))
        {
            Debug.LogAssertion("Attempting to access coordinate outside of the board space");
            return null;
        }

        return  boardPositions[coordinates.x][coordinates.y].unit;
    }

    public UnitBehaviour[] GetAllUnits()
    {
        List<UnitBehaviour> allUnits = new List<UnitBehaviour>();
        for(int column = 0; column < boardPositions.Length; column++)
        {
            for (int row = 0; row < boardPositions[column].Length; row++)
            {
                BoardPosition position = boardPositions[column][row];
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
        BoardPosition boardPosition = boardPositions[coordinates.x][coordinates.y];
        boardPosition.unit = unitBehaviour;

        var newPos = new Vector3(boardPosition.worldSpacePosition.x, boardPosition.worldSpacePosition.y, coordinates.y);
        
        if (unitBehaviour != null)
        {
            unitBehaviour.coordinates = coordinates;
            unitBehaviour.isMovable = false;
            unitBehaviour.transform.DOMove(newPos, blockSwapTime).SetEase(Ease.InQuad).OnComplete(() =>
            {
                unitBehaviour.transform.position = newPos;
                unitBehaviour.isMovable = true;
            });
        }

        boardPositions[coordinates.x][coordinates.y] = boardPosition;
    }
    
    public void SetHeroUnitBehaviour(int newColumn, UnitBehaviour unitBehaviour)
    {
        BoardPosition boardPosition = heroPositions[newColumn];
        boardPosition.unit = unitBehaviour;

        var newPos = new Vector3(boardPosition.worldSpacePosition.x, boardPosition.worldSpacePosition.y, 10);
        
        if (unitBehaviour != null)
        {
            unitBehaviour.isMovable = false;
            unitBehaviour.transform.DOMove(newPos, blockSwapTime).SetEase(Ease.InQuad).OnComplete(() =>
            {
                unitBehaviour.transform.position = newPos;
                unitBehaviour.isMovable = true;
            });
        }

        Heroes[newColumn] = unitBehaviour;
        heroPositions[newColumn] = boardPosition;
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
    
    public void SwapHeroBlocks(int heroColumn1, int heroColumn2)
    {
        // Cache original blocks
        var originalLeftBlock = BoardManager.Instance.GetHeroUnitBehaviourAtCoordinate(heroColumn1);
        var originalRightBlock = BoardManager.Instance.GetHeroUnitBehaviourAtCoordinate(heroColumn2);
    
        // Now you can swap the blocks
        SetHeroUnitBehaviour(heroColumn1, originalRightBlock);
        SetHeroUnitBehaviour(heroColumn2, originalLeftBlock);
    }
    
    public bool AreColumnsFull()
    {
        for (int column = 0; column < _numColumns; column++)
        {
            for (var row = 0; row < _numRows; row++)
            {
                if (boardPositions[column][row].unit == null)
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

        return boardPositions[coordinates.x][coordinates.y].worldSpacePosition;

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
        for (int i = 0; i < heroPositions.Length; i++)
        {
            if (heroPositions[i].unit == null)
            {
                Heroes[i] = heroUnit;
                heroPositions[i].unit = heroUnit;
                return heroPositions[i].worldSpacePosition;
            }
        }

        Debug.LogAssertion("Attempting to add a hero, but there isn't room");
        return null;
    }

    public BoardManager.Coordinates? FindUnitBehaviour(UnitBehaviour unit)
    {
        for (int i = 0; i < boardPositions.Length; i++)
        {
            for (int j = 0; j < boardPositions[i].Length; j++)
            {
                if (boardPositions[i][j].unit == unit)
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
        boardPositions[unitCoords.Value.x][unitCoords.Value.y].unit = null;
    }

    public UnitBehaviour GetHeroFromColumn(int column)
    {
        return Heroes[column];
    }
    
}
