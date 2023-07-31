using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using Random = UnityEngine.Random;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    public GameObject smokePrefab;
    public GameObject rubbleBlock;
    public GameObject bombPrefab;
    public GameObject deployObject;

    public int numRows;
    public int numColumns;

    public GameObject boardGameObject;
    
    // A list of all possible blocks that can be spawned on the board
    public List<GameObject> blockGameObjects = new List<GameObject>();

    // An empty block for filling blank space in the board
    public GameObject emptyBlock;
    
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
    private Dictionary<int, List<Coordinates>> _selectorPositions = new Dictionary<int, List<Coordinates>>();
    public bool canMove;
    
    public struct Coordinates 
    {
        public int x;

        public int y;

        public Coordinates(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Coordinates other)
        {
            if (other.x == x && other.y == y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public enum Direction
    {
        Up, 
        Down,
        Left,
        Right
    }

    public enum BlockLayout
    {
        Surrounding,
        Crossing
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
        canMove = true;
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


        for (int currentColumn = 0; currentColumn < numColumns; currentColumn++) {
            // How many rows should be initialized with blocks?
            int filledRows = UnityEngine.Random.Range(1, numRows + 1);

            // instantiate blocks at corresponding spawn positions
            for (int currentRow = 0; currentRow < numRows; currentRow++)
            {
                // get the x and y spawn coordinates 
                float spawnX = BlockPositions[currentColumn, currentRow].x;
                float spawnY = BlockPositions[currentColumn, currentRow].y;

                Block leftBlock = null;
                Block bottomBlock = null;

                // Don't spawn either of these blocks -- this will prevent starting with matches
                if (currentColumn - 1 >= 0 && _board[currentColumn - 1, currentRow])
                {
                    leftBlock = _board[currentColumn - 1, currentRow].GetComponent<Block>();
                }

                if (currentRow - 1 >= 0 && _board[currentColumn, currentRow - 1])
                {
                    bottomBlock = _board[currentColumn, currentRow - 1].GetComponent<Block>();

                }

                // create Vector3 coordinate at which to spawn the block
                Vector3 spawnPos = new Vector3(spawnX, spawnY, 1);

                if (currentRow < filledRows)
                {
                    GameObject toInstantiate = GetRandomBlock(except: leftBlock, or: bottomBlock);

                    GameObject block = Instantiate(toInstantiate, spawnPos, Quaternion.identity, this.transform);
                    _board[currentColumn, currentRow] = block;
                }
                else 
                {
                    // If we are past the filledRows, start inserting empty blocks instead
                    _board[currentColumn, currentRow] = Instantiate(emptyBlock, spawnPos, Quaternion.identity, this.transform); 
                }
            }
        }
        EventManager.Instance.BoardReady();
    }

    public List<Coordinates> GetRandomCoordinates(int numCoords)
    {
        List<Coordinates> coordinatesList = new List<Coordinates>();
        int maxAttempts = 10;
        int attempts = 0;

        while (coordinatesList.Count < numCoords && attempts < maxAttempts)
        {
            attempts++;
            int randomX = Random.Range(0, numColumns);
            int randomY = Random.Range(0, numRows);
            Coordinates coords = new Coordinates(randomX, randomY);
            if (!coordinatesList.Contains(coords))
            {
                coordinatesList.Add(coords);
            }
        }
        
        return coordinatesList;
    }

    public void ReplaceWithRandomBlock(Coordinates coordinates)
    {
        Block leftBlock = null;
        Block bottomBlock = null;

        // Don't spawn either of these blocks -- this will prevent starting with matches
        if (coordinates.x - 1 >= 0)
        {
            leftBlock = _board[coordinates.x - 1, coordinates.y].GetComponent<Block>();
        }

        if (coordinates.y - 1 >= 0)
        {
            bottomBlock = _board[coordinates.x, coordinates.y - 1].GetComponent<Block>();
        }

        GameObject newBlock = GetRandomBlock(leftBlock, bottomBlock);
        ReplaceBlock(coordinates, newBlock);
    }

    private GameObject GetRandomBlock(Block except, Block or)
    {
        int randomNum = Random.Range(0, blockGameObjects.Count);
        GameObject blockToReturn = blockGameObjects[randomNum];
        return blockToReturn;
    }

    public void SwapBlocks(Coordinates leftBlockCoords, Coordinates rightBlockCoords, int playerIndex)
    {
        // Retrieve blocks from board based on their grid coordinates
        GameObject leftBlock = _board[leftBlockCoords.x, leftBlockCoords.y];
        GameObject rightBlock = _board[rightBlockCoords.x, rightBlockCoords.y];
        AudioManager.Instance.PlayWithRandomPitch("whoosh");
               
        // Swap the blocks data
        _board[leftBlockCoords.x, leftBlockCoords.y] = rightBlock;
        _board[rightBlockCoords.x, rightBlockCoords.y] = leftBlock;

        // Cache the value of the left block's position before it 
        // moves to the right block's position
        Vector3 originalPos = leftBlock.transform.position;

        // Swap the gameobjects' positions
        StartCoroutine(LerpBlocks(leftBlock, rightBlock));
            
        ApplyGravity();
    }

    public void ReplaceBlock(Coordinates block, GameObject newBlockPrefab)
    {
        GameObject newBlock;
        if (!newBlockPrefab.activeInHierarchy)
        {
            newBlock = Instantiate(newBlockPrefab);
        }
        else
        {
            newBlock = newBlockPrefab;
        }
        GameObject blockToReplace = _board[block.x, block.y];
        _board[block.x, block.y] = newBlock;
        newBlock.transform.SetParent(boardGameObject.transform);
        newBlock.transform.position = blockToReplace.transform.position;
        Destroy(blockToReplace);
    }

    public void ReplaceBlocks(Coordinates leftBlockCoords, Coordinates rightBlockCoords, GameObject newBlockPrefab)
    {
        // Retrieve blocks from board based on their grid coordinates
        GameObject leftBlock = _board[leftBlockCoords.x, leftBlockCoords.y];
        GameObject rightBlock = _board[rightBlockCoords.x, rightBlockCoords.y];
        
        _board[leftBlockCoords.x, leftBlockCoords.y] = Instantiate(newBlockPrefab, boardGameObject.transform);
        _board[leftBlockCoords.x, leftBlockCoords.y].GetComponent<Block>().SetCoordinates(leftBlockCoords);
        _board[rightBlockCoords.x, rightBlockCoords.y] = Instantiate(newBlockPrefab, boardGameObject.transform);
        _board[rightBlockCoords.x, rightBlockCoords.y].GetComponent<Block>().SetCoordinates(rightBlockCoords);

        _board[leftBlockCoords.x, leftBlockCoords.y].transform.position = leftBlock.transform.position;
        Destroy(leftBlock);
        
        _board[rightBlockCoords.x, rightBlockCoords.y].transform.position = rightBlock.transform.position;
        Destroy(rightBlock);
        
    }
    
    public void ReplaceBlocks(Coordinates blockCoords, GameObject newBlock)
    {
        // Retrieve blocks from board based on their grid coordinates
        GameObject block = _board[blockCoords.x, blockCoords.y];

        _board[blockCoords.x, blockCoords.y] = newBlock;
        newBlock.transform.SetParent(boardGameObject.transform);

        _board[blockCoords.x, blockCoords.y].transform.position = block.transform.position;
        Destroy(block);
        
    }

    private bool CheckMatch(int playerIndex)
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
                        StartCoroutine(MatchFound(blockCoordinates, playerIndex));
                        return true;
                    } 

                    blockCoordinates.Clear();

                    CheckVertical(block, currCoords, blockCoordinates);
                    if (blockCoordinates.Count >= 3)
                    {
                        StartCoroutine(MatchFound(blockCoordinates, playerIndex));
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

    private void ApplyGravity()
    {
        for (int column = 0; column < numColumns; column++)
        {
            List<Block> collapsedBlocks = new List<Block>();
            for (int row = 0; row < numRows; row++)
            {
                Block b = GetBlock(new Coordinates(column, row));
                if (b.unit != null)
                {
                    collapsedBlocks.Add(b);
                }
            }

            for (int newRow = 0; newRow < numRows; newRow++)
            {
                Vector3 newPosition = BlockPositions[column, newRow];
                if (newRow < collapsedBlocks.Count)
                {
                    // Instruct all non-empty blocks to be compressed to the bottom of the board
                    Block b = collapsedBlocks[newRow];
                    b.targetPosition = newPosition;
                    _board[column, newRow] = b.gameObject;
                }
                else
                {
                    _board[column, newRow] = Instantiate(emptyBlock, newPosition, Quaternion.identity, transform);
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

    private List<Block> Chain(Block origin)
    {
        return new List<Block>();
    }

    private void HandleMatch(List<Coordinates> blocksInRun, int playerIndex)
    {
        MatchHandler.Instance.ShowMatchDisplayerUI();
        isRefilling = true;
        Block type = _board[blocksInRun[0].x, blocksInRun[0].y].GetComponent<Block>();
        HandleCombo(AccumulatePoints(type, blocksInRun.Count), playerIndex);

        foreach (Coordinates coords in blocksInRun)
        {
            GameObject block = _board[coords.x, coords.y];
            _board[coords.x, coords.y] = null;
        }
    }
    
    private IEnumerator MatchFound(List<Coordinates> blocksInRun, int playerIndex)
    {
        canMove = false;
        combo += 1;
        foreach (var block in blocksInRun)
        {
            GameObject blockGO = GetBlockGameObject(block);
            blockGO.GetComponent<Block>().Match();
        }
        HandleMatch(blocksInRun, playerIndex);
        yield return new WaitForSeconds(1.5f);
        RefillBoard(blocksInRun);
        if (!CheckMatch(playerIndex)) 
        {
            isRefilling = false;
            ResolveCombo(playerIndex);
        }  
    }

    private int AccumulatePoints(Block block, int runSize)
    {
        // int pointvalue = block.pointValue;
        int pointvalue = 10;

        switch (runSize)
        {
            case 3:
                return pointvalue * 3;

            case 4:
                return (pointvalue * pointvalue);

            case 5:
                return (int)(pointvalue * (pointvalue * 1.5));

            default: 
                return 0;
        }
    }

    private void HandleCombo(int points, int playerIndex)
    {
        if (combo == 1)
        {
            AudioManager.Instance.Play("chime");
            accumulatedPoints += points;
            //AudioManager.Instance.Play("chime");
        } 

        else if (combo == 2)
        {
            AudioManager.Instance.Play("chime2");
            accumulatedPoints += points * 2;
            //AudioManager.Instance.Play("chime2");
        } 

        else if (combo == 3)
        {
            AudioManager.Instance.Play("chime3");
            accumulatedPoints += points * 3;
            //AudioManager.Instance.Play("ding");
        } 

        else if (combo >= 4)
        {
            AudioManager.Instance.Play("chime3");
            accumulatedPoints += points * 4;
            //AudioManager.Instance.Play("tada");
        }
    }

    private void ResolveCombo(int playerIndex)
    {
        //Player.Instance.UpdateScore(accumulatedPoints);
        accumulatedPoints = 0;
        accumulatedScores.Clear();
        combo = 0;
        
        // TODO: Hide Display Card
        MatchHandler.Instance.HideMatchDisplayerUI();
        canMove = true;
    }

    private GameObject GetBlockGameObject(Coordinates blockCoords)
    {
        if (blockCoords.x < numColumns && blockCoords.y < numRows)
        {
            return _board[blockCoords.x, blockCoords.y];
        }
        else return null;
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

    public void SetSelectorPosition(int playerIndex, Coordinates pos1, Coordinates pos2)
    {
        if (_selectorPositions.ContainsKey(playerIndex)) _selectorPositions.Remove(playerIndex);
        List<Coordinates> coords = new List<Coordinates>();
        coords.Add(pos1);
        coords.Add(pos2);
        _selectorPositions.Add(playerIndex, coords);
    }

    public Player GetPlayerFromPosition(Coordinates affectedCoordinates)
    {
        Player p = null;
        foreach (var player in GameManager.Instance.playersInGame)
        {
            int playerIndex = player.playerIndex;
            if (_selectorPositions.ContainsKey(playerIndex))
            {
                List<Coordinates> occupiedCoordinates = _selectorPositions[playerIndex];
                foreach (var coords in occupiedCoordinates)
                    {
                        if (coords.Equals(affectedCoordinates))
                        {
                            p = player;
                        }
                    }
            }
        }

        return p;
    }

    private IEnumerator LerpBlocks(GameObject block1, GameObject block2)
    {
        float elapsedTime = 0;
        float waitTime = .2f;

        Vector3 targetPosition1 = block2.transform.position;
        Vector3 targetPosition2 = block1.transform.position;

        Vector3 currentPos1 = block1.transform.position;
        Vector3 currentPos2 = block2.transform.position;
        
        while (elapsedTime < waitTime)
        {
            block1.transform.position = Vector3.Lerp(currentPos1, targetPosition1, (elapsedTime / waitTime));
            block2.transform.position = Vector3.Lerp(currentPos2, targetPosition2, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
 
            // Yield here
            yield return null;
        }  
        // Make sure we got there
        block1.transform.position = targetPosition1;
        block2.transform.position = targetPosition2;
        yield return null;
        ApplyGravity();
    }
    
    public bool IsSelectorColliding(BoardManager.Coordinates block1, BoardManager.Coordinates block2)
    {
        bool isColliding = false;

        foreach (var player in GameManager.Instance.playersInGame)
        {
            if ((block1.Equals(player.selector.pivotBlockCoordinates) || (block1.Equals(player.selector.rotatingBlockCoordinates)) || block2.Equals(player.selector.pivotBlockCoordinates) || (block2.Equals(player.selector.rotatingBlockCoordinates))))
            {
                isColliding = true;
            }
        }
        return isColliding;
    }

    public Block GetBlock(Coordinates coordinates)
    {
        return GetBlockGameObject(coordinates).GetComponent<Block>();
    }

    public void SpawnSmoke(Coordinates coordinates)
    {
        GameObject smoke = Instantiate(smokePrefab);
        smoke.transform.position = _board[coordinates.x, coordinates.y].transform.position;

        smoke.GetComponent<Animator>().SetTrigger("neutral");
        
        StartCoroutine(ISpawnSmoke(smoke));
    }

    public void SpawnSmoke(Coordinates coordinates, int playerIndex)
    {
        GameObject smoke = Instantiate(smokePrefab);
        smoke.transform.position = _board[coordinates.x, coordinates.y].transform.position;

        string trigger = "blue";
        switch (playerIndex)
        {
            case 0:
                trigger = "blue";
                break;
            case 1:
                trigger = "red";
                break;
            case 2:
                trigger = "purple";
                break;
            case 3:
                trigger = "orange";
                break;
        }
        smoke.GetComponent<Animator>().SetTrigger(trigger);
        
        StartCoroutine(ISpawnSmoke(smoke));
    }

    private IEnumerator ISpawnSmoke(GameObject smoke)
    {
        yield return new WaitForSeconds(.7f);
        Destroy(smoke);
    }
    
    private IEnumerator ReplaceBlock(Coordinates coordinates, Block block, float time)
    {
        yield return new WaitForSeconds(time);
        GameObject rubbleBlock = Instantiate(this.rubbleBlock);
        _board[coordinates.x, coordinates.y] = rubbleBlock;
        rubbleBlock.transform.position = block.transform.position;
        Destroy(block.gameObject);
    }

    public List<Coordinates> GetNeighboringCoordinates(Coordinates coordinates, BlockLayout blockLayout)
    {
        List<Coordinates> neighbors = new List<Coordinates>();
        int x = coordinates.x;
        int y = coordinates.y;
        if (blockLayout == BlockLayout.Surrounding)
        {
            // west
            if (x - 1 >= 0) neighbors.Add(new Coordinates(x - 1, y));

            // northwest
            if (x - 1 >= 0 && y + 1 < numRows) neighbors.Add(new Coordinates(x - 1, y + 1));

            // north
            if (y + 1 < numRows) neighbors.Add(new Coordinates(x, y + 1));

            // northeast
            if (x + 1 < numColumns && y + 1 < numRows) neighbors.Add(new Coordinates(x + 1, y + 1));

            // east
            if (x + 1 < numColumns) neighbors.Add(new Coordinates(x + 1, y));

            // southeast 
            if (x + 1 < numColumns && y - 1 >= 0) neighbors.Add(new Coordinates(x + 1, y - 1));
            
            // south
            if (y - 1 >= 0) neighbors.Add(new Coordinates(x, y - 1));
            
            // southwest
            if (x - 1 >= 0 && y - 1 >= 0) neighbors.Add(new Coordinates(x - 1, y - 1));
        }

        else
        {
            for (int row = 0; row < numRows; row++)
            {
                Coordinates toAdd = new Coordinates(x, row);
                neighbors.Add(toAdd);
            }
            
            for (int column = 0; column < numRows; column++)
            {
                Coordinates toAdd = new Coordinates(column, y);
                neighbors.Add(toAdd);
            }
        }

        return neighbors;
    }

    public void ShowDeployAnimation(Coordinates coordinates)
    {
        StartCoroutine(IShowDeployAnimation(coordinates));
    }

    private IEnumerator IShowDeployAnimation(Coordinates coordinates)
    {
        GameObject spawnedAnimation = Instantiate(deployObject);
        spawnedAnimation.transform.position = _board[coordinates.x, coordinates.y].transform.position;
        yield return new WaitForSeconds(.6f);
        Destroy(spawnedAnimation);
    }
}