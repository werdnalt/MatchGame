using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Selector : MonoBehaviour
{
    public GameObject selectorBlock;

    private string Name;
    private BoardManager.Coordinates leftBlockCoordinates;
    private BoardManager.Coordinates rightBlockCoordinates;
    private BoardManager.Coordinates prevLeftBlockCoordinates;
    private BoardManager.Coordinates prevRightBlockCoordinates;
    private GameObject[,] selectorBlocks;
    private int numRows;
    private int numColumns;

    public GameObject p1SelectorPrefab;
    public GameObject p2SelectorGameobject;

    private GameObject _selectorObject;

    private GameObject _leftBlock;
    private GameObject _rightBlock;

    // Start is called before the first frame update
    void Start()
    {
        CreateAndSetSelector();

        
        
        
        numRows = BoardManager.Instance.numRows;
        numColumns = BoardManager.Instance.numColumns;

        selectorBlocks = new GameObject[numColumns, numRows];

        leftBlockCoordinates = new BoardManager.Coordinates(numColumns / 2, numRows / 2);
        if (leftBlockCoordinates.x + 1 <= BoardManager.Instance.numColumns)
        {
            rightBlockCoordinates = new BoardManager.Coordinates(leftBlockCoordinates.x + 1, leftBlockCoordinates.y);
        } else {
            Debug.Log("Right block position is out of bounds. Add more columns");
        }

        //InitializeSelectorBlocks();
        //Highlight();
    }

    void CreateAndSetSelector()
    {
        _selectorObject = Instantiate(p1SelectorPrefab);
        leftBlockCoordinates = new BoardManager.Coordinates(0, 0);
        rightBlockCoordinates = new BoardManager.Coordinates(0, 1);
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
                if (leftBlockCoordinates.x > 0)
                {
                    MoveSelector(-1, 0);
                }
            }
            // move right
            else
            {
                // Check to make sure right selector isn't going off
                // right side of grid
                if (rightBlockCoordinates.x + 1 < numColumns)
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
                if (leftBlockCoordinates.y > 0)
                {
                    MoveSelector(0, -1);
                }
            }
            // move up
            else
            {
                // Check to make sure both selectors aren't going off
                // the top of the grid
                if (leftBlockCoordinates.y + 1 < numRows)
                {
                    MoveSelector(0, 1);
                }
            }
        }
    }

    void OnSelect(InputValue inputValue)
    {
        BoardManager.Instance.SwapBlocks(leftBlockCoordinates, rightBlockCoordinates);
    }

    private void MoveSelector(int xAdjustment, int yAdjustment)
    {
        //if NOT refilling, run this code
     if (!BoardManager.Instance.isRefilling)
     {
//            AudioManager.Instance.Play("click");
            leftBlockCoordinates = new BoardManager.Coordinates(leftBlockCoordinates.x + xAdjustment, leftBlockCoordinates.y + yAdjustment);
            rightBlockCoordinates = new BoardManager.Coordinates(rightBlockCoordinates.x + xAdjustment, rightBlockCoordinates.y + yAdjustment);
            SetSelectorPosition();
      }
    }

    private void SetSelectorPosition()
    {
        _selectorObject.transform.position =
            BoardManager.Instance.GetSelectorPosition(leftBlockCoordinates, rightBlockCoordinates);
    }


}
