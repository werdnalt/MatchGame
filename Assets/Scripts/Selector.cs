using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class Selector : MonoBehaviour
{
    private string Name;
    public BoardManager.Coordinates pivotBlockCoordinates;
    public BoardManager.Coordinates rotatingBlockCoordinates;
    
    private BoardManager.Coordinates prevPivotBlockCoordinates;
    private BoardManager.Coordinates prevRotatingBlockCoordinates;
    
    private int numRows;
    private int numColumns;
    private DirectionFromPivot _directionFromPivot;

    public GameObject p1SelectorPrefab;
    public GameObject p2SelectorPrefab;
    public GameObject p3SelectorPrefab;
    public GameObject p4SelectorPrefab;
    
    private GameObject _selectorObject;

    private GameObject _pivotBlock;
    private GameObject _rotatingBlock;
    private float _timeOfLastSwap;
    private float _timeBetweenSwaps = .5f;
    private int _playerIndex;

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum DirectionFromPivot
    {
        North,
        East,
        South,
        West
    }

    private void Awake()
    {
        EventManager.Instance.onBoardReady += CreateAndSetSelector;
        _timeOfLastSwap = -Mathf.Infinity;
        _playerIndex = GetComponent<PlayerInput>().playerIndex;
    }

    private void Start()
    {

    }

    public void Setup()
    {
        

        //InitializeSelectorBlocks();
        //Highlight();
    }

    void CreateAndSetSelector()
    {
        numRows = BoardManager.Instance.numRows;
        numColumns = BoardManager.Instance.numColumns;
        _directionFromPivot = DirectionFromPivot.East;
        
        int id = GetComponent<PlayerInput>().playerIndex;
        switch (id)
        {
            case 0:
                _selectorObject = Instantiate(p1SelectorPrefab);
                break;
            case 1:
                _selectorObject = Instantiate(p1SelectorPrefab);
                break;
            case 2:
                _selectorObject = Instantiate(p1SelectorPrefab);
                break;
            case 3:
                _selectorObject = Instantiate(p1SelectorPrefab);
                break;
        }
        
        pivotBlockCoordinates = new BoardManager.Coordinates(0, 0);
        rotatingBlockCoordinates = new BoardManager.Coordinates(1, 0);
        SetSelectorPosition();
    }
    
    void OnMove(InputValue inputValue)
    {
        Vector2 movement = inputValue.Get<Vector2>();

        float absX = Mathf.Abs(movement.x);
        float absY = Mathf.Abs(movement.y);

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
    
        void OnRotateClockwise(InputValue inputValue)
        {
            switch (_directionFromPivot)
            {
                case DirectionFromPivot.East:
                    if (CanMove(Direction.Down))
                    {
                        rotatingBlockCoordinates =
                            new BoardManager.Coordinates(pivotBlockCoordinates.x, pivotBlockCoordinates.y - 1);
                        RotateSelector();
                        SetSelectorPosition();
                        _directionFromPivot = DirectionFromPivot.South;
                    }
                    break;
                
                case DirectionFromPivot.West:
                    if (CanMove(Direction.Up))
                    {
                        rotatingBlockCoordinates =
                            new BoardManager.Coordinates(pivotBlockCoordinates.x, pivotBlockCoordinates.y + 1);
                        RotateSelector();
                        SetSelectorPosition();
                        _directionFromPivot = DirectionFromPivot.North;
                    }
                    break;
                
                case DirectionFromPivot.North:
                    if (CanMove(Direction.Right))
                    {
                        rotatingBlockCoordinates =
                            new BoardManager.Coordinates(pivotBlockCoordinates.x + 1, pivotBlockCoordinates.y);
                        RotateSelector();
                        SetSelectorPosition();
                        _directionFromPivot = DirectionFromPivot.East;
                    }
                    break;
                
                case DirectionFromPivot.South:
                    if (CanMove(Direction.Left))
                    {
                        rotatingBlockCoordinates =
                            new BoardManager.Coordinates(pivotBlockCoordinates.x - 1, pivotBlockCoordinates.y);
                        RotateSelector();
                        SetSelectorPosition();
                        _directionFromPivot = DirectionFromPivot.West;
                    }
                    break;
            }
        }
        
void OnRotateCounterClockwise(InputValue inputValue)
        {
            switch (_directionFromPivot)
            {
                case DirectionFromPivot.East:
                    if (CanMove(Direction.Up))
                    {
                        rotatingBlockCoordinates =
                            new BoardManager.Coordinates(pivotBlockCoordinates.x, pivotBlockCoordinates.y + 1);
                        RotateSelector();
                        SetSelectorPosition();
                        _directionFromPivot = DirectionFromPivot.North;
                    }
                    break;
                
                case DirectionFromPivot.West:
                    if (CanMove(Direction.Down))
                    {
                        rotatingBlockCoordinates =
                            new BoardManager.Coordinates(pivotBlockCoordinates.x, pivotBlockCoordinates.y - 1);
                        RotateSelector();
                        SetSelectorPosition();
                        _directionFromPivot = DirectionFromPivot.South;
                    }
                    break;
                
                case DirectionFromPivot.North:
                    if (CanMove(Direction.Left))
                    {
                        rotatingBlockCoordinates =
                            new BoardManager.Coordinates(pivotBlockCoordinates.x - 1, pivotBlockCoordinates.y);
                        RotateSelector();
                        SetSelectorPosition();
                        _directionFromPivot = DirectionFromPivot.West;
                    }
                    break;
                
                case DirectionFromPivot.South:
                    if (CanMove(Direction.Right))
                    {
                        rotatingBlockCoordinates =
                            new BoardManager.Coordinates(pivotBlockCoordinates.x + 1, pivotBlockCoordinates.y);
                        RotateSelector();
                        SetSelectorPosition();
                        _directionFromPivot = DirectionFromPivot.East;
                    }
                    break;
            }
        }

    void OnSelect(InputValue inputValue)
    {
        if (BoardManager.Instance != null && CanSwap())
        {
            BoardManager.Instance.SwapBlocks(pivotBlockCoordinates, rotatingBlockCoordinates, _playerIndex);
            _timeOfLastSwap = Time.time;
        }
    }

    void OnSpecialAbility(InputValue inputValue)
    {
        
    }

    private void MoveSelector(int xAdjustment, int yAdjustment)
    {
        BoardManager.Coordinates newCoords1 = new BoardManager.Coordinates(pivotBlockCoordinates.x + xAdjustment,
            pivotBlockCoordinates.y + yAdjustment);
        BoardManager.Coordinates newCoords2 = new BoardManager.Coordinates(rotatingBlockCoordinates.x + xAdjustment,
            rotatingBlockCoordinates.y + yAdjustment);
        
        //if NOT refilling, run this code
        if (!BoardManager.Instance.isRefilling )
     {
            AudioManager.Instance.PlayWithRandomPitch("move");
            pivotBlockCoordinates = newCoords1;
            rotatingBlockCoordinates = newCoords2;

            BoardManager.Instance.CheckBlock(pivotBlockCoordinates);
            BoardManager.Instance.CheckBlock(rotatingBlockCoordinates);
            
            SetSelectorPosition();
      }
    }

    private void SetSelectorPosition()
    {
        _selectorObject.transform.position =
            BoardManager.Instance.GetSelectorPosition(pivotBlockCoordinates, rotatingBlockCoordinates);
    }

    private void RotateSelector()
    {
        _selectorObject.transform.RotateAround(transform.position, Vector3.forward, 90);
    }

    public BoardManager.Coordinates[] GetSelectedBlocks()
    {
        BoardManager.Coordinates[] selectedBlocks = new BoardManager.Coordinates[2];
        selectedBlocks[0] = pivotBlockCoordinates;
        selectedBlocks[1] = rotatingBlockCoordinates;
        return selectedBlocks;
    }

    private bool CanMove(Direction direction)
    {
        if (BoardManager.Instance.canMove)
        {
            switch (direction)
            {
                case Direction.Up:
                    int largestY = Math.Max(pivotBlockCoordinates.y, rotatingBlockCoordinates.y);
                    if (largestY + 1 < numRows) return true;
                    break;
            
                case Direction.Down:
                    int smallestY = Math.Min(pivotBlockCoordinates.y, rotatingBlockCoordinates.y);
                    if (smallestY > 0) return true;
                    break;
            
                case Direction.Left:
                    int smallestX = Math.Min(pivotBlockCoordinates.x, rotatingBlockCoordinates.x);
                    if (smallestX > 0) return true;
                    break;
            
                case Direction.Right:
                    int largestX = Math.Max(pivotBlockCoordinates.x, rotatingBlockCoordinates.x);
                    if (largestX + 1 < numColumns) return true;
                    break;
            }
        }

        return false;
    }

    private bool CanSwap()
    {
        if (Time.time - _timeOfLastSwap >= _timeBetweenSwaps && BoardManager.Instance.canMove)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
