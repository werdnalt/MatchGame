using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public int numRows;
    public int numColumns; 
    // A list of all possible blocks that can be spawned on the board
    public List<GameObject> blocks = new List<GameObject>();

    // A 2D array that contains all of the possible positions a block can be spawned
    public Vector2[,] spawnPositions;
    // A 2D array that contains the blocks that comprise the board
    private GameObject[,] board;
    private float blockSize;

    public struct Coordinates 
    {
        public int x;
        public int y;

        public Coordinates(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    void Awake()
    {
        spawnPositions = new Vector2[numColumns, numRows];
        board = new GameObject[numColumns, numRows];

        if (Instance == null) Instance = this;

        InitializeBoard();
    }

    // Iterate through each row of the board one by one. For each column in the row, 
    // populate it with a Block.
    private void InitializeBoard()
    {
        int offset = 250;

        // Choose a starting position to begin populating spawn coordinates
        float startXPos = transform.position.x + offset;
        float startYPos = transform.position.y + offset;

        blockSize = blocks[0].GetComponent<RectTransform>().rect.height;
        
        // populate array with spawn positions
        for (int currentRow = 0; currentRow < numRows; currentRow++)
        {
            for (int currentColumn = 0; currentColumn < numColumns; currentColumn++)
            {
                Vector2 pos = new Vector2(startXPos + (blockSize * currentColumn), startYPos + (blockSize * currentRow));
                spawnPositions[currentColumn, currentRow] = pos;
            }
        }

        // instantiate blocks at corresponding spawn positions
        for (int currentRow = 0; currentRow < numRows; currentRow++)
        {
            for (int currentColumn = 0; currentColumn < numColumns; currentColumn++)
            {
                // get the x and y spawn coordinates 
                float spawnX = spawnPositions[currentColumn, currentRow].x;
                float spawnY = spawnPositions[currentColumn, currentRow].y;

                Block leftBlock = null;
                Block bottomBlock = null;

                // Don't spawn either of these blocks -- this will prevent starting with matches
                if (currentColumn - 1 >= 0)
                {
                    leftBlock = board[currentColumn - 1, currentRow].GetComponent<Block>();
                }

                if (currentRow - 1 >= 0)
                {
                    bottomBlock = board[currentColumn, currentRow - 1].GetComponent<Block>();
                }

                // create Vector3 coordinate at which to spawn the block
                Vector3 spawnPos = new Vector3(spawnX, spawnY, 1);

                GameObject toInstantiate = GetRandomBlock(except: leftBlock, or: bottomBlock);

                GameObject block = Instantiate(toInstantiate, Camera.main.ScreenToWorldPoint(spawnPos), Quaternion.identity, this.transform);
                board[currentColumn, currentRow] = block;
            }
        }

        CheckMatch();
    }

    private GameObject GetRandomBlock(Block except, Block or)
    {
        int randomNum = Random.Range(0, blocks.Count);
        GameObject blockToReturn = blocks[randomNum];

        if (except)
        {
            if (blockToReturn.GetComponent<Block>().blockType == except.blockType)
            {
                return GetRandomBlock(except, or);
            }
        }

        if (or)
        {
            if (blockToReturn.GetComponent<Block>().blockType == or.blockType)
            {
                return GetRandomBlock(except, or);
            }
        }

        return blockToReturn;
    }

    public void SwapBlocks(Coordinates leftBlockCoords, Coordinates rightBlockCoords)
    { 
        // Retrieve blocks from board based on their grid coordinates
        GameObject leftBlock = board[leftBlockCoords.x, leftBlockCoords.y];
        GameObject rightBlock = board[rightBlockCoords.x, rightBlockCoords.y];

        // Swap the blocks data
        board[leftBlockCoords.x, leftBlockCoords.y] = rightBlock;
        board[rightBlockCoords.x, rightBlockCoords.y] = leftBlock;

        // Cache the value of the let block's position before it 
        // moves to the right block's position
        Vector3 originalPos = leftBlock.transform.position;

        // Swap the gameobjects' positions
        leftBlock.transform.position = rightBlock.transform.position;
        rightBlock.transform.position = originalPos;

        CheckMatch();
    }

    private void CheckMatch()
    {
        for (int row = 0; row < numRows; row++)
        {
            for (int column = 0; column < numColumns; column++)
            {
                GameObject currBlockGameObject = board[column, row];

                if (currBlockGameObject)
                {
                    Coordinates currCoords = new Coordinates(column, row);
                    Block block = currBlockGameObject.GetComponent<Block>();

                    List<GameObject> horizontalBlocks = new List<GameObject>();
                    CheckHorizontal(block, currCoords, horizontalBlocks);
                    if (horizontalBlocks.Count >= 3) HandleMatch(horizontalBlocks);


                    List<GameObject> verticalBlocks = new List<GameObject>();
                    CheckVertical(block, currCoords, verticalBlocks);
                    if (verticalBlocks.Count >= 3) HandleMatch(verticalBlocks);
                }
            }
        }
    }

    private void CheckHorizontal(Block block, Coordinates toCheck, List<GameObject> horizontalBlocks)
    {
        horizontalBlocks.Add(block.gameObject);
        if (toCheck.x + 1 < numColumns)
        {
            Coordinates coordsOfComparison = new Coordinates(toCheck.x + 1, toCheck.y);
            Block toCompare = board[coordsOfComparison.x, coordsOfComparison.y].GetComponent<Block>();
            if (block.blockType == toCompare.blockType)
            {
                CheckHorizontal(toCompare, coordsOfComparison, horizontalBlocks);
            }
        }
    }

    private void CheckVertical(Block block, Coordinates toCheck, List<GameObject> verticalBlocks)
    {
        verticalBlocks.Add(block.gameObject);
        if (toCheck.y + 1 < numRows)
        {
            Coordinates coordsOfComparison = new Coordinates(toCheck.x, toCheck.y + 1);
            Block toCompare = board[coordsOfComparison.x, coordsOfComparison.y].GetComponent<Block>();
            if (block.blockType == toCompare.blockType)
            {
                CheckVertical(toCompare, coordsOfComparison, verticalBlocks);
            }
        }
    }

    private void HandleMatch(List<GameObject> blocksInRun)
    {
        switch (blocksInRun.Count)
        {
            case 3:
                Debug.Log("OHHHYEAH!!");
                break;
                    
            case 4:
                break;

            case 5:
                break;
        }

        foreach (GameObject go in blocksInRun)
        {
            Destroy(go);
        }
    }
}