using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

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
    public bool isRefilling;
    public TextMeshProUGUI accumulatedPointsText;

    private GameObject[,] board;
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

    void Awake()
    {
        accumulatedPointsText.gameObject.SetActive(false);
        spawnPositions = new Vector2[numColumns, numRows];
        board = new GameObject[numColumns, numRows];
        isRefilling = false;

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

        if (leftBlock.GetComponent<Block>().blockType != Block.Type.Invincible && 
            rightBlock.GetComponent<Block>().blockType != Block.Type.Invincible)
            {
                AudioManager.Instance.Play("woosh");

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
            } else {
                AudioManager.Instance.Play("error");
            }


    }

    private bool CheckMatch()
    {
        for (int row = 0; row < numRows; row++)
        {
            for (int column = 0; column < numColumns; column++)
            {
                GameObject currBlockGameObject = board[column, row];

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
            Block toCompare = board[coordsOfComparison.x, coordsOfComparison.y].GetComponent<Block>();
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
            Block toCompare = board[coordsOfComparison.x, coordsOfComparison.y].GetComponent<Block>();
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
                if (board[column, row] == null)
                {
                    bool replacementNeeded = true;
                    Coordinates refilling = new Coordinates(column, row);
                    Vector3 moveTo = Camera.main.ScreenToWorldPoint(new Vector3(spawnPositions[column, row].x, spawnPositions[column, row].y, 1)); 
                    // iterate through the cells above it to find the next non-empty cell
                    for (int searchRow = row + 1; searchRow < numRows; searchRow++)
                    {
                        // if you find a cell that isn't empty, replace the empty cell with its block
                        if (board[column, searchRow] != null && replacementNeeded)
                        {
                            GameObject block = board[column, searchRow];
                            board[column, searchRow] = null;
                            board[refilling.x, refilling.y] = block;
                            block.transform.position = moveTo;
                            //Vector2.Lerp(block.transform.position, (spawnPositions[column, row]), Time.deltaTime);
                            replacementNeeded = false;
                        }
                    }
                    if (replacementNeeded)
                    {
                        GameObject createdBlock = Instantiate(GetRandomBlock(null, null), Camera.main.ScreenToWorldPoint(spawnPositions[column, numRows - 1]), Quaternion.identity, this.transform);
                        board[refilling.x, refilling.y] = createdBlock;
                        createdBlock.transform.position = moveTo;
                        // Vector2.Lerp(createdBlock.transform.position, Camera.main.ScreenToWorldPoint(spawnPositions[column, row]), Time.deltaTime);
                    }
                }
            }
        }
    }

    private void HandleMatch(List<Coordinates> blocksInRun)
    {
        isRefilling = true;
        Block type = board[blocksInRun[0].x, blocksInRun[0].y].GetComponent<Block>();
        HandleCombo(AccumulatePoints(type, blocksInRun.Count));
        MatchHandler.Instance.ResolveEffect(type, blocksInRun.Count);

        foreach (Coordinates coords in blocksInRun)
        {
            GameObject block = board[coords.x, coords.y];
            board[coords.x, coords.y] = null;
            Destroy(block);
        }
    }

    private IEnumerator MatchFound(List<Coordinates> blocksInRun)
    {
        combo += 1;
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
            AudioManager.Instance.Play("chime");
        } 

        else if (combo == 2)
        {
            accumulatedPoints += points * 2;
            AudioManager.Instance.Play("chime2");
        } 

        else if (combo == 3)
        {
            accumulatedPoints += points * 3;
            AudioManager.Instance.Play("ding");
        } 

        else if (combo >= 4)
        {
            accumulatedPoints += points * 4;
            AudioManager.Instance.Play("tada");
        }

        accumulatedScores.Add(points);
        StartCoroutine(ShowAccumulatedPoints());
    }

    private IEnumerator ShowAccumulatedPoints()
    {
        string accumulatedPointsString = "";
        for (int i = 0; i < accumulatedScores.Count; i++)
        {
            if (i == 0)
            {
                accumulatedPointsString += accumulatedScores[i].ToString();
            } else {
                int multiplier = Mathf.Clamp(i, 2, 4);
                accumulatedPointsString += " + " + accumulatedScores[i].ToString() + "(x" + multiplier + ")";
            }
        }
        accumulatedPointsText.gameObject.SetActive(true);
        accumulatedPointsText.text = accumulatedPointsString;

        yield return new WaitForSeconds(1.5f);
        accumulatedPointsText.gameObject.SetActive(false);
    }

    private void ResolveCombo()
    {
        Player.Instance.UpdateScore(accumulatedPoints);
        accumulatedPoints = 0;
        accumulatedScores.Clear();
        combo = 0;
    }
}