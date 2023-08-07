using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
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

    public List<GameObject> cellPrefabs;

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

    private List<GameObject> _objectsToDestroyLater = new List<GameObject>();
    private GameObject[,] cellObjects;
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
        isRefilling = false;

        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        AttackTimeManager.instance.attackTriggerListeners += PerformCombat;
        
        EventManager.Instance.LevelLoaded();
        canMove = true;
        
        CreateBoard(); // creates the actual grid composed of square gameobjects. unit placement will be based on these transforms
    }

    private void AddBlock(Unit unit)
    {
        // create Block from the unit
        var blockGameObject = CreateBlock(unit);

        // find available cell on grid for block
        var blockCoordinates = FindBlockPlacement();

        // assign block to board
        _board.SetBlock(blockCoordinates, blockGameObject);

        // drop block gameobject into position
        DropBlock(blockGameObject, _board.GetUnitPosition(blockCoordinates));
    }

    private void DropBlock(GameObject blockGameobject, Vector3 position)
    {
        var dropFrom = new Vector3(position.x, Camera.main.orthographicSize + 1, position.z);

        blockGameobject.transform.position = dropFrom;
        
        // Determine drop speed
        float dropSpeed = 1.0f; // Unity units per second

        // Use DoTween to move the unit to the final position
        blockGameobject.transform.DOMove(position, dropSpeed).SetEase(Ease.OutBounce);
    }

    // create the game board where pieces will be populated. whenever the value of numColumns or numRows is changed, the
    // game board should repopulate while in edit mode
    
    // the board should originate from the gameobject from which this script is attached, so it can be dragged around and
    // readjusted while in edit mode
    private void CreateBoard()
    {
        _board = new Board(numColumns, numRows + 2);
        cellObjects = new GameObject[numColumns, numRows + 2];

        // Calculate the total width and height of the board
        float boardWidth = numColumns; // Assuming each cell has a width of 1 unit
        float boardHeight = numRows + 2; // Assuming each cell has a height of 1 unit

        // Calculate the starting position for the board to be centered on the screen
        float startX = -boardWidth / 2 + 0.5f; // +0.5f since we assume each cell has a width of 1 unit and we want to start from its center
        float startY = -boardHeight / 2 + 0.5f; // +0.5f for the same reason as above

        for (int column = 0; column < numColumns; column++)
        {
            for (int row = 0; row < numRows + 2; row++)
            {
                // Calculate the position for each cell based on the start position
                float xPos = startX + column;
                float yPos = startY + row;
            
                GameObject cellGameObject = Instantiate(cellPrefabs[(column + row) % 2], new Vector3(xPos, yPos, 0), Quaternion.identity, gameObject.transform);
                cellGameObject.name = $"Square ({column},{row})";
                cellObjects[column, row] = cellGameObject;
                _board.SetUnitPositions(new Coordinates(column, row), cellGameObject);

                // Hide game objects in the 2nd row
                if (row == 1)
                {
                    Renderer renderer = cellGameObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.enabled = false;
                    }
                }
            }
        }

        EventManager.Instance.BoardReady();
    }

    public void SpawnWave(Wave wave)
    {
        StartCoroutine(SpawnUnit(wave));
    }

    private IEnumerator SpawnUnit(Wave wave)
    {
        for (var unitsSpawned = 0; unitsSpawned < wave.waveSize; unitsSpawned++)
        {
            var randomUnitFromWave = wave.units[Random.Range(0, wave.units.Count)];
            AddBlock(randomUnitFromWave);
            yield return new WaitForSeconds(.1f);
        }
    }
    
    private Coordinates FindBlockPlacement()
    {
        // if all columns are full, overfill a column
        
        // otherwise, try a random column
        return _board.FindRandomColumn();

        // if it has space, the block can go in the column

        // if not, try another column

        // assign unit to Board data

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

    private GameObject CreateBlock(Unit unit)
    {
        var blockInstance = Instantiate(blockPrefab);

        // hydrate generic block prefab 
        blockInstance.GetComponent<Block>().Initialize(unit);

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