using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    private GameObject[,] _board;
    private int _numColumns;
    private int _numRows;

    private List<Unit> _frontRowEnemyPositions;
    private List<Unit> _heroPositions;
    
    public Board(int numColumns, int numRows)
    {
        _board = new GameObject[numColumns, numRows];
        _numColumns = numColumns;
        _numRows = numRows;
    }
    
    public GameObject GetBlockGameObject(BoardManager.Coordinates coordinates)
    {
        return _board[coordinates.x, coordinates.y] ? _board[coordinates.x, coordinates.y] : null;
    }
    
    public Block GetBlock(BoardManager.Coordinates coordinates)
    {
        if (coordinates.x >= 0 && coordinates.x < _board.GetLength(0) &&
            coordinates.y >= 0 && coordinates.y < _board.GetLength(1))
        {
            var component = _board[coordinates.x, coordinates.y].GetComponent<Block>();
            return component != null ? component : null;
        }
        else
        {
            Debug.Log("out of bounds");
            return null;
        }
    }

    public void SetBlock(BoardManager.Coordinates coordinates, GameObject block)
    {
        _board[coordinates.x, coordinates.y] = block;

        // unit is in the front/attacking row
        if (coordinates.y == 0)
        {
            _frontRowEnemyPositions[coordinates.x] = block.GetComponent<Block>().unit;
        }
    }

    public void SetCoordinates(BoardManager.Coordinates coordinates)
    {
        var blockGameObject = _board[coordinates.x, coordinates.y];
        if (blockGameObject && blockGameObject.GetComponent<Block>())
        {
            blockGameObject.GetComponent<Block>().coordinates = coordinates;
        }
    }

    public void SwapBlocks(BoardManager.Coordinates leftBlockCoords, BoardManager.Coordinates rightBlockCoords)
    {
        SetBlock(leftBlockCoords, GetBlockGameObject(rightBlockCoords));
        SetBlock(rightBlockCoords, GetBlockGameObject(leftBlockCoords));
    }
}
