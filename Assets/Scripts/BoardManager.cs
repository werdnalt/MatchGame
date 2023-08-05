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

    public GameObject blockPrefab;
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
    private Board _board;
    
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
        _board = new Board(numColumns, numRows);
        isRefilling = false;

        if (Instance == null) Instance = this;

        InitializeBoard();
    }

    private void Start()
    {
        AttackTimeManager.instance.attackTriggerListeners += PerformCombat;
        
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

                var currentCoordinates = new Coordinates(currentColumn, currentRow);

                Block leftBlock = null;
                Block bottomBlock = null;

                // Don't spawn either of these blocks -- this will prevent starting with matches
                if (currentColumn - 1 >= 0 && _board.GetBlockGameObject(new Coordinates(currentColumn - 1, currentRow)));
                {
                    leftBlock = _board.GetBlock(new Coordinates(currentColumn - 1, currentRow));
                }

                if (currentRow - 1 >= 0 && _board.GetBlockGameObject(new Coordinates(currentColumn, currentRow - 1))) ;
                {
                    bottomBlock = _board.GetBlock(new Coordinates(currentColumn, currentRow - 1));
                }

                // create Vector3 coordinate at which to spawn the block
                Vector3 spawnPos = new Vector3(spawnX, spawnY, 1);

                if (currentRow < filledRows)
                {
                    var block = CreateBlock();
                    block.transform.SetParent(this.gameObject.transform);
                    block.transform.position = spawnPos;
                    _board.SetBlock(currentCoordinates, block);
                }
                else 
                {
                    // If we are past the filledRows, start inserting empty blocks instead
                    _board.SetBlock(currentCoordinates, Instantiate(emptyBlock, spawnPos, Quaternion.identity, this.transform)); 
                }
            }
        }
        EventManager.Instance.BoardReady();
    }

    private void CreateHeroes()
    {
        // for each column, create a hero spot
        
        // populate it with a hero from the available heroes
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
    
    public void SwapBlocks(Coordinates leftBlockCoords, Coordinates rightBlockCoords, int playerIndex)
    {
        // Retrieve blocks from board based on their grid coordinates
        GameObject leftBlock = _board.GetBlockGameObject(leftBlockCoords);
        GameObject rightBlock = _board.GetBlockGameObject(rightBlockCoords);
        AudioManager.Instance.PlayWithRandomPitch("whoosh");
               
        // Swap the blocks data
        _board.SwapBlocks(leftBlockCoords, rightBlockCoords);

        // Cache the value of the left block's position before it 
        // moves to the right block's position
        Vector3 originalPos = leftBlock.transform.position;

        // Swap the gameobjects' positions
        StartCoroutine(LerpBlocks(leftBlock, rightBlock));
            
        ApplyGravity();
    }

    public void ReplaceBlock(Coordinates blockCoords, GameObject newBlockPrefab)
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
        GameObject blockToReplace = _board.GetBlockGameObject(blockCoords);
        _board.SetBlock(blockCoords, newBlock);
        newBlock.transform.SetParent(boardGameObject.transform);
        newBlock.transform.position = blockToReplace.transform.position;
        Destroy(blockToReplace);
    }
    
    private void ApplyGravity()
    {
        for (int column = 0; column < numColumns; column++)
        {
            List<Block> collapsedBlocks = new List<Block>();
            for (int row = 0; row < numRows; row++)
            {
                var b = _board.GetBlock(new Coordinates(column, row));
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
                    var b = collapsedBlocks[newRow];
                    b.targetPosition = newPosition;
                    _board.SetBlock(new Coordinates(column, newRow), b.gameObject);
                }
                else
                {
                    _board.SetBlock(new Coordinates(column, newRow), Instantiate(emptyBlock, newPosition, Quaternion.identity, transform));
                }
            }
        }
    }

    private List<Block> Chain(Block origin)
    {
        return GetNeighboringCoordinates(origin.coordinates).Select((coordinate) 
            => _board.GetBlock(coordinate)).Where((block) 
            => block.unit == origin.unit).SelectMany(Chain).ToList();
    }
    
    // Pass in the coordinates of the two blocks the selector is highlighting.
    // This will then return the transform in the middle of the two where the selector gameobject should be rendered
    public Vector3 GetSelectorPosition(Coordinates leftBlockCoords, Coordinates rightBlockCoords)
    {
        var lBlock = _board.GetBlockGameObject(leftBlockCoords);
        var rBlock = _board.GetBlockGameObject(rightBlockCoords);

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
    
    public List<Coordinates> GetNeighboringCoordinates(Coordinates coordinates)
    {
        List<Coordinates> neighbors = new List<Coordinates>();
        int x = coordinates.x;
        int y = coordinates.y;

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

        return neighbors;
    }

    private GameObject CreateBlock()
    {
        // get unit from current wave
        var randomUnitFromWave = WaveManager.Instance.GetRandomUnitFromWave();

        var blockInstance = Instantiate(blockPrefab);

        // hydrate generic block prefab 
        blockInstance.GetComponent<Block>().Initialize(randomUnitFromWave);

        return blockInstance;
    }

    public Block GetBlock(Coordinates coordinates)
    {
        return _board.GetBlock(coordinates);
    }

    public void PerformCombat()
    {
        Debug.Log("Performing combat");
        
        var heroes = _board.HeroPositions;
        var enemies = _board.FrontRowEnemyPositions;
        // heroes attack first
        for (var i = 0; i < numColumns; i++)
        {
            if (!heroes[i] || !enemies[i]) return;
            
            enemies[i].TakeDamage(heroes[i].unit.attack);
        }
        
        // then enemies
        for (var i = 0; i < numColumns; i++)
        {
            if (!heroes[i] || !enemies[i]) return;
            
            if (enemies[i].currentHp > 0) enemies[i].TakeDamage(heroes[i].unit.attack);
        }
    }
}