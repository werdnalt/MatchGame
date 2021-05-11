using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    // Start is called before the first frame update
    void Start()
    {
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

        InitializeSelectorBlocks();
        Highlight();
    }

    void InitializeSelectorBlocks()
    {
        for (int row = 0; row < numRows; row++)
        {
            for (int column = 0; column < numColumns; column++)
            {
                Vector3 spawnPos = new Vector3(BoardManager.Instance.spawnPositions[column, row].x, BoardManager.Instance.spawnPositions[column, row].y, 1);
                selectorBlocks[column, row] = Instantiate(selectorBlock, Camera.main.ScreenToWorldPoint(spawnPos), Quaternion.identity, this.transform);
                selectorBlocks[column, row].gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        // move selector up
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Check to make sure both selectors aren't going off
            // the top of the grid
            if (leftBlockCoordinates.y + 1 < numRows)
            {
                MoveSelector(0, 1);
            }
        }

        // move selector left
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // Check to make sure left selector isn't going off the left 
            // side of the grid
            if (leftBlockCoordinates.x > 0)
            {
                MoveSelector(-1, 0);
            }
        }

        // move selector right
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            // Check to make sure right selector isn't going off
            // right side of grid
            if (rightBlockCoordinates.x + 1 < numColumns)
            {
                MoveSelector(1, 0);
            }
        }

        // move selector down
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Check to make sure both selectors aren't going off
            // bottom side of grid
            if (leftBlockCoordinates.y > 0)
            {
                MoveSelector(0, -1);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            BoardManager.Instance.SwapBlocks(leftBlockCoordinates, rightBlockCoordinates);
        }
    }

    private void MoveSelector(int xAdjustment, int yAdjustment)
    {
        if (!BoardManager.Instance.isRefilling)
        {
            AudioManager.Instance.Play("click");
            leftBlockCoordinates = new BoardManager.Coordinates(leftBlockCoordinates.x + xAdjustment, leftBlockCoordinates.y + yAdjustment);
            rightBlockCoordinates = new BoardManager.Coordinates(rightBlockCoordinates.x + xAdjustment, rightBlockCoordinates.y + yAdjustment);
            Highlight();
        }
    }

    private void Highlight()
    {
        selectorBlocks[prevLeftBlockCoordinates.x, prevLeftBlockCoordinates.y].gameObject.SetActive(false);
        selectorBlocks[prevRightBlockCoordinates.x, prevRightBlockCoordinates.y].gameObject.SetActive(false);

        prevLeftBlockCoordinates = leftBlockCoordinates;
        prevRightBlockCoordinates = rightBlockCoordinates;

        selectorBlocks[leftBlockCoordinates.x, leftBlockCoordinates.y].gameObject.SetActive(true);
        selectorBlocks[rightBlockCoordinates.x, rightBlockCoordinates.y].gameObject.SetActive(true);
    }



}
