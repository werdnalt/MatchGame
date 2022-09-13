using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public int numRows;
    public int numColumns; 
    
    // A list of all possible blocks that can be spawned on the board
    public List<GameObject> blockGameObjects = new List<GameObject>();
    
    // A 2D array that contains all of the possible positions a block can be spawned
    public Vector2[,] BlockPositions;
    
    // A 2D array that contains the blocks that comprise the board
    private GameObject[,] _board;
    
    public bool isRefilling;
    public TextMeshProUGUI accumulatedPointsText;


    private float blockSize;

    private int combo = 0;
    private int accumulatedPoints = 0;
    private List<int> accumulatedScores = new List<int>();

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

    public enum Direction
    {
        Up, 
        Down,
        Left,
        Right
    }

    void Awake()
    {
        BlockPositions = new Vector2[numColumns, numRows];
        _board = new GameObject[numColumns, numRows];
        isRefilling = false;

        if (Instance == null) Instance = this;

        InitializeBoard();
    }

    private void Start()
    {
        EventManager.Instance.LevelLoaded();
    }

    // Iterate through each row of the board one by one. For each column in the row, 
    // populate it with a Block.
    private void InitializeBoard()
    {
        int offset = 0;

        // Choose a starting position to begin populating spawn coordinates
        float startXPos = transform.position.x + offset;
        float startYPos = transform.position.y + offset;

        blockSize = .9f;
        Debug.Log(blockSize);
        //blockSize = 90;
        
        // populate array with spawn positions
        for (int currentRow = 0; currentRow < numRows; currentRow++)
        {
            for (int currentColumn = 0; currentColumn < numColumns; currentColumn++)
            {
                Vector2 pos = new Vector2(startXPos + (blockSize * currentColumn), startYPos + (blockSize * currentRow));
                BlockPositions[currentColumn, currentRow] = pos;
            }
        }

        // instantiate blocks at corresponding spawn positions
        for (int currentRow = 0; currentRow < numRows; currentRow++)
        {
            for (int currentColumn = 0; currentColumn < numColumns; currentColumn++)
            {
                // get the x and y spawn coordinates 
                float spawnX = BlockPositions[currentColumn, currentRow].x;
                float spawnY = BlockPositions[currentColumn, currentRow].y;

                Block leftBlock = null;
                Block bottomBlock = null;

                // Don't spawn either of these blocks -- this will prevent starting with matches
                if (currentColumn - 1 >= 0)
                {
                    leftBlock = _board[currentColumn - 1, currentRow].GetComponent<Block>();
                }

                if (currentRow - 1 >= 0)
                {
                    bottomBlock = _board[currentColumn, currentRow - 1].GetComponent<Block>();
                }

                // create Vector3 coordinate at which to spawn the block
                Vector3 spawnPos = new Vector3(spawnX, spawnY, 1);

                GameObject toInstantiate = GetRandomBlock(except: leftBlock, or: bottomBlock);

                GameObject block = Instantiate(toInstantiate, spawnPos, Quaternion.identity, this.transform);
                _board[currentColumn, currentRow] = block;
            }
        }

        CheckMatch();
    }

    private GameObject GetRandomBlock(Block except, Block or)
    {
        int randomNum = Random.Range(0, blockGameObjects.Count);
        GameObject blockToReturn = blockGameObjects[randomNum];

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
        GameObject leftBlock = _board[leftBlockCoords.x, leftBlockCoords.y];
        GameObject rightBlock = _board[rightBlockCoords.x, rightBlockCoords.y];

        if (leftBlock.GetComponent<Block>().blockType != Block.Type.Invincible && 
            rightBlock.GetComponent<Block>().blockType != Block.Type.Invincible)
            {
//                AudioManager.Instance.Play("woosh");


                // Swap the blocks data
                _board[leftBlockCoords.x, leftBlockCoords.y] = rightBlock;
                _board[rightBlockCoords.x, rightBlockCoords.y] = leftBlock;

                // Cache the value of the left block's position before it 
                // moves to the right block's position
                Vector3 originalPos = leftBlock.transform.position;

                // Swap the gameobjects' positions
                leftBlock.transform.position = rightBlock.transform.position;
                rightBlock.transform.position = originalPos;

                CheckMatch();
            } else {
                //AudioManager.Instance.Play("error");
            }
    }

    public void ReplaceBlocks(Coordinates leftBlockCoords, Coordinates rightBlockCoords, GameObject newBlockPrefab)
    {
        // Retrieve blocks from board based on their grid coordinates
        GameObject leftBlock = _board[leftBlockCoords.x, leftBlockCoords.y];
        GameObject rightBlock = _board[rightBlockCoords.x, rightBlockCoords.y];

        leftBlock = newBlockPrefab;
        rightBlock = newBlockPrefab;
    }

    private bool CheckMatch()
    {
        for (int row = 0; row < numRows; row++)
        {
            for (int column = 0; column < numColumns; column++)
            {
                GameObject currBlockGameObject = _board[column, row];

                if (currBlockGameObject)
                {
                    Coordinates currCoords = new Coordinates(column, row);
                    List<Coordinates> blockCoordinates = new List<Coordinates>();
                    Block block = currBlockGameObject.GetComponent<Block>();
;                   
                    CheckHorizontal(block, currCoords, blockCoordinates);
                    if (blockCoordinates.Count >= 3)
                    {
                        StartCoroutine(MatchFound(blockCoordinates));
                        return true;
                    } 

                    blockCoordinates.Clear();

                    CheckVertical(block, currCoords, blockCoordinates);
                    if (blockCoordinates.Count >= 3)
                    {
                        StartCoroutine(MatchFound(blockCoordinates));
                        return true;
                    } 
                }
            }
        }
    return false;
    }

    private void CheckHorizontal(Block block, Coordinates toCheck, List<Coordinates> horizontalBlocks)
    {
        horizontalBlocks.Add(toCheck);
        if (toCheck.x + 1 < numColumns)
        {
            Coordinates coordsOfComparison = new Coordinates(toCheck.x + 1, toCheck.y);
            Block toCompare = _board[coordsOfComparison.x, coordsOfComparison.y].GetComponent<Block>();
            if (block.blockType == toCompare.blockType && toCompare)
            {
                CheckHorizontal(toCompare, coordsOfComparison, horizontalBlocks);
            }
        }
    }

    private void CheckVertical(Block block, Coordinates toCheck, List<Coordinates> verticalBlocks)
    {
        verticalBlocks.Add(toCheck);
        if (toCheck.y + 1 < numRows)
        {
            Coordinates coordsOfComparison = new Coordinates(toCheck.x, toCheck.y + 1);
            Block toCompare = _board[coordsOfComparison.x, coordsOfComparison.y].GetComponent<Block>();
            if (block.blockType == toCompare.blockType && toCompare)
            {
                CheckVertical(toCompare, coordsOfComparison, verticalBlocks);
            }
        }
    }

    private void RefillBoard(List<Coordinates> toRefill)
    {
        isRefilling = true;
        foreach (Coordinates coords in toRefill)
        {   
            int column = coords.x;
            // for each cell going up the grid
            for (int row = coords.y; row < numRows; row++)
            {
                // if the cell is empty
                if (_board[column, row] == null)
                {
                    bool replacementNeeded = true;
                    Coordinates refilling = new Coordinates(column, row);
                    Vector3 moveTo = (new Vector3(BlockPositions[column, row].x, BlockPositions[column, row].y, 1)); 
                    // iterate through the cells above it to find the next non-empty cell
                    for (int searchRow = row + 1; searchRow < numRows; searchRow++)
                    {
                        // if you find a cell that isn't empty, replace the empty cell with its block
                        if (_board[column, searchRow] != null && replacementNeeded)
                        {
                            GameObject block = _board[column, searchRow];
                            _board[column, searchRow] = null;
                            _board[refilling.x, refilling.y] = block;
                            //block.transform.position = moveTo;
                            StartCoroutine(MoveBlock(block, block.transform.position, moveTo, .2f));
                            //Vector2.Lerp(block.transform.position, moveTo, .5f);
                            replacementNeeded = false;
                        }
                    }
                    if (replacementNeeded)
                    {
                        GameObject createdBlock = Instantiate(GetRandomBlock(null, null), (BlockPositions[column, numRows - 1]), Quaternion.identity, this.transform);
                        _board[refilling.x, refilling.y] = createdBlock;
                        createdBlock.transform.position = moveTo;
                        StartCoroutine(MoveBlock(createdBlock, createdBlock.transform.position, moveTo, .2f));
                        // Vector2.Lerp(createdBlock.transform.position, Camera.main.ScreenToWorldPoint(spawnPositions[column, row]), Time.deltaTime);
                    }
                }
            }
        }
    }

    private IEnumerator MoveBlock(GameObject block, Vector2 startingPos, Vector2 endingPos, float duration)
    {
        float time = 0;
        
        while (time < duration && block)
        {
            block.transform.position = Vector2.Lerp(startingPos, endingPos, time/duration);
            time += Time.deltaTime;
            yield return null;
        }

        if (block) block.transform.position = endingPos;
    }

    private void HandleMatch(List<Coordinates> blocksInRun)
    {
        MatchHandler.Instance.ShowMatchDisplayerUI();
        isRefilling = true;
        Block type = _board[blocksInRun[0].x, blocksInRun[0].y].GetComponent<Block>();
        HandleCombo(AccumulatePoints(type, blocksInRun.Count));
        MatchHandler.Instance.QueueEffect(type, blocksInRun.Count);

        foreach (Coordinates coords in blocksInRun)
        {
            GameObject block = _board[coords.x, coords.y];
            _board[coords.x, coords.y] = null;
        }
    }

    private IEnumerator MatchFound(List<Coordinates> blocksInRun)
    {
        combo += 1;
        foreach (var block in blocksInRun)
        {
            GameObject blockGO = GetBlockGameObject(block);
            blockGO.GetComponent<Block>().Match();
        }
        HandleMatch(blocksInRun);
        yield return new WaitForSeconds(1.5f);
        RefillBoard(blocksInRun);
        if (!CheckMatch()) 
        {
            isRefilling = false;
            ResolveCombo();
        }  
    }

    private int AccumulatePoints(Block block, int runSize)
    {
        int pointvalue = block.pointValue;

        switch (runSize)
        {
            case 3:
                return pointvalue * 3;

            case 4:
                return (pointvalue * pointvalue);

            case 5:
                return (pointvalue * pointvalue * pointvalue);

            default: 
                return 0;
        }

    }

    private void HandleCombo(int points)
    {
        if (combo == 1)
        {
            accumulatedPoints += points;
            //AudioManager.Instance.Play("chime");
        } 

        else if (combo == 2)
        {
            accumulatedPoints += points * 2;
            //AudioManager.Instance.Play("chime2");
        } 

        else if (combo == 3)
        {
            accumulatedPoints += points * 3;
            //AudioManager.Instance.Play("ding");
        } 

        else if (combo >= 4)
        {
            accumulatedPoints += points * 4;
            //AudioManager.Instance.Play("tada");
        }

        accumulatedScores.Add(points);
    }

    private void ResolveCombo()
    {
        //Player.Instance.UpdateScore(accumulatedPoints);
        accumulatedPoints = 0;
        accumulatedScores.Clear();
        combo = 0;
        
        // TODO: Hide Display Card
        MatchHandler.Instance.HideMatchDisplayerUI();
    }

    private GameObject GetBlockGameObject(Coordinates blockCoords)
    {
        return _board[blockCoords.x, blockCoords.y];
    }

    // Pass in the coordinates of the two blocks the selector is highlighting.
    // This will then return the transform in the middle of the two where the selector gameobject should be rendered
    public Vector3 GetSelectorPosition(Coordinates leftBlock, Coordinates rightBlock)
    {
        GameObject lBlock = GetBlockGameObject(leftBlock);
        GameObject rBlock = GetBlockGameObject(rightBlock);

        Vector2 middlePos;
        float x = rBlock.transform.position.x - ((rBlock.transform.position.x - lBlock.transform.position.x) / 2);
        float y = rBlock.transform.position.y - ((rBlock.transform.position.y - lBlock.transform.position.y) / 2);
        middlePos = new Vector3(x, y, -50);
        return middlePos;
    }
}