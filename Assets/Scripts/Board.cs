using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    private List<List<GameObject>> _board;
    private List<List<GameObject>> _unitPositions;
    private int _numColumns;
    private int _numRows;
    private int _fullColumnCount;

    public Block[] FrontRowEnemyPositions { get; }
    public Block[] HeroPositions { get; }

    public Board(int numColumns, int numRows)
    {
        _board = new List<List<GameObject>>();
        _unitPositions = new List<List<GameObject>>();
        _numColumns = numColumns;
        _numRows = numRows;
        FrontRowEnemyPositions = new Block[numColumns];
        HeroPositions = new Block[numColumns];
        _fullColumnCount = _numColumns + 2; // this accounts for hero and blank row
    }
    
    public GameObject GetBlockGameObject(BoardManager.Coordinates coordinates)
    {
        return _board[coordinates.x][coordinates.y] ? _board[coordinates.x][coordinates.y] : null;
    }
    
    public Block GetBlock(BoardManager.Coordinates coordinates)
    {
        if (_board[coordinates.x][coordinates.y])
        {
            return _board[coordinates.x][coordinates.y].GetComponent<Block>();
        }
        else
        {
            return null;
        }
    }

    public void SetBlock(BoardManager.Coordinates coordinates, GameObject block)
    {
        _board[coordinates.x][coordinates.y] = block;

        // unit is in the front/attacking row
        if (coordinates.y == 0)
        {
            FrontRowEnemyPositions[coordinates.x] = block.GetComponent<Block>();
        }
    }

    public void SetUnitPositions(BoardManager.Coordinates coordinates, GameObject positionAnchor)
    {
        _unitPositions[coordinates.x][coordinates.y] = positionAnchor;
    }

    public void SetCoordinates(BoardManager.Coordinates coordinates)
    {
        var blockGameObject = _board[coordinates.x][coordinates.y];
        if (blockGameObject && blockGameObject.GetComponent<Block>())
        {
            blockGameObject.GetComponent<Block>().coordinates = coordinates;
        }
    }

    public void SwapBlocks(BoardManager.Coordinates leftBlockCoords, BoardManager.Coordinates rightBlockCoords)
    {
        // Cache original blocks
        var originalLeftBlock = GetBlockGameObject(leftBlockCoords);
        var originalRightBlock = GetBlockGameObject(rightBlockCoords);
    
        // Now you can swap the blocks
        SetBlock(leftBlockCoords, originalRightBlock);
        SetBlock(rightBlockCoords, originalLeftBlock);
    }
    
    public bool AreColumnsFull()
    {
        var full = true;
        

        for (int column = 0; column < _numColumns; column++)
        {
            for (var row = 2; row < _fullColumnCount; row++)
            {
                if (_board[column][row].GetComponent<Block>().unit != null) continue;
                full = false;
                break;
            }
        }
        
        return full;
    }

    public BoardManager.Coordinates FindRandomColumn()
    {
        List<int> availableColumnIndices = new List<int>();
        BoardManager.Coordinates cellCoordinates = new BoardManager.Coordinates(0, 0);

        // find any columns that have at least one available cell
        for (int column = 0; column < _numColumns; column++)
        {
            for (var row = 2; row < _fullColumnCount; row++)
            {
                if (_board[column][row].GetComponent<Block>().unit == null)
                {
                    availableColumnIndices.Add(column);
                }
            }
        }
        
        // pick a random column from the available columns
        var chosenColumnIndex = availableColumnIndices[Random.Range(0, availableColumnIndices.Count)];

        var chosenColumn = _board[chosenColumnIndex];

        // add the block to that column, adding its data to the board
        for (var row = 2; row < _fullColumnCount; row++)
        {
            if (chosenColumn[row].GetComponent<Block>().unit == null)
            {
                cellCoordinates = new BoardManager.Coordinates(chosenColumnIndex, row);
            }
        }

        return cellCoordinates;
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

}
