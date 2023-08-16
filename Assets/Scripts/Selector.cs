using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Selector : MonoBehaviour
{
    public BoardManager.Coordinates _leftBlockCoordinates;
    public BoardManager.Coordinates _rightBlockCoordinates;
    
    private int _numRows;
    private int _numColumns;

    public GameObject selectorPrefab;
    private GameObject _selectorInstance;

    private GameObject _pivotBlock;
    private GameObject _rotatingBlock;

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private void Start()
    {
        CreateAndSetSelector();
    }

    private void CreateAndSetSelector()
    {
        _selectorInstance = Instantiate(selectorPrefab);
        
        _numRows = BoardManager.Instance.numRows;
        _numColumns = BoardManager.Instance.numColumns;

        _leftBlockCoordinates = new BoardManager.Coordinates(_numColumns / 2,  _numRows / 2);
        _rightBlockCoordinates = new BoardManager.Coordinates(_leftBlockCoordinates.x + 1, _leftBlockCoordinates.y);
        
        SetSelectorPosition();
    }
    
    void OnMove(InputValue inputValue)
    {
        if (!GameManager.Instance.canMove) return;
        
        var movement = inputValue.Get<Vector2>();

        var absX = Mathf.Abs(movement.x);
        var absY = Mathf.Abs(movement.y);

        if (absX > absY)
        {
            // move left
            if (movement.x < 0)
            {
                // Check to make sure left selector isn't going off the left 
                // side of the grid
                if (CanMove(Direction.Left))
                {
                    MoveSelector(-1, 0);
                }
            }
            // move right
            else
            {
                // Check to make sure right selector isn't going off
                // right side of grid
                if (CanMove(Direction.Right))
                {
                    MoveSelector(1, 0);
                }
            }
        }
        else if (absY > absX)
        {
            // move down
            if (movement.y < 0)
            {
                // Check to make sure both selectors aren't going off
                // bottom side of grid
                if (CanMove(Direction.Down))
                {
                    MoveSelector(0, -1);
                }
            }
            // move up
            else
            {
                // Check to make sure both selectors aren't going off
                // the top of the grid
                if (CanMove(Direction.Up))
                {
                    MoveSelector(0, 1);
                }
            }
        }
    }
    
    void OnSelect(InputValue inputValue)
    {
        if (!GameManager.Instance.canMove) return;
        if (!BoardManager.Instance) return;

        
        if (BoardManager.Instance.GetBlock(_leftBlockCoordinates).isMovable &&
            BoardManager.Instance.GetBlock(_rightBlockCoordinates).isMovable)
        {
            BoardManager.Instance.SwapBlocks(_leftBlockCoordinates, _rightBlockCoordinates);
        }
        
    }

    private void MoveSelector(int xAdjustment, int yAdjustment)
    {
        var newCoords1 = new BoardManager.Coordinates(_leftBlockCoordinates.x + xAdjustment,
            _leftBlockCoordinates.y + yAdjustment);
        var newCoords2 = new BoardManager.Coordinates(_rightBlockCoordinates.x + xAdjustment,
            _rightBlockCoordinates.y + yAdjustment);
        
        //AudioManager.Instance.PlayWithRandomPitch("move");
        
        _leftBlockCoordinates = newCoords1;
        _rightBlockCoordinates = newCoords2;

        BoardManager.Instance.SetSelectorPosition(newCoords1, newCoords2);
        
        SetSelectorPosition();
    }

    private void SetSelectorPosition()
    {
        _selectorInstance.transform.position =
            BoardManager.Instance.GetSelectorPosition(_leftBlockCoordinates, _rightBlockCoordinates);
    }
    
    public BoardManager.Coordinates[] GetSelectedBlocks()
    {
        BoardManager.Coordinates[] selectedBlocks = new BoardManager.Coordinates[2];
        selectedBlocks[0] = _leftBlockCoordinates;
        selectedBlocks[1] = _rightBlockCoordinates;
        return selectedBlocks;
    }

    private bool CanMove(Direction direction)
    {
        if (BoardManager.Instance.canMove)
        {
            switch (direction)
            {
                case Direction.Up:
                    int largestY = Math.Max(_leftBlockCoordinates.y, _rightBlockCoordinates.y);
                    if (largestY + 1 < _numRows) return true;
                    break;
            
                case Direction.Down:
                    int smallestY = Math.Min(_leftBlockCoordinates.y, _rightBlockCoordinates.y);
                    if (smallestY > 0) return true;
                    break;
            
                case Direction.Left:
                    int smallestX = Math.Min(_leftBlockCoordinates.x, _rightBlockCoordinates.x);
                    if (smallestX > 0) return true;
                    break;
            
                case Direction.Right:
                    int largestX = Math.Max(_leftBlockCoordinates.x, _rightBlockCoordinates.x);
                    if (largestX + 1 < _numColumns) return true;
                    break;
            }
        }

        return false;
    }
}
