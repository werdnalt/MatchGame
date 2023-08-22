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
    
    // A 2D array that contains the blocks that comprise the board
    private Board _board;
    
    public bool isRefilling;
    public TextMeshProUGUI accumulatedPointsText;


    private float blockSize;

    private Player _player;
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
        isRefilling = false;

        if (Instance == null) Instance = this;
        
        CreateBoard(); // creates the actual grid composed of square gameobjects. unit placement will be based on these transforms
    }

    private void Start()
    {
        AttackTimeManager.instance.attackTriggerListeners += PerformCombat;

        _player = GameObject.FindObjectOfType<Player>();
        
        EventManager.Instance.LevelLoaded();
        canMove = true;

        CreateHeroes();
    }

    private void AddBlock(Unit unit)
    {
        // create unit behavior for the unit
        var unitBehaviour = CreateBlock(unit);

        // find available board position on grid for unit
        var blockCoordinates = FindBlockPlacement();

        // TODO: fix this
        if (blockCoordinates == null || blockCoordinates.Equals(new Coordinates(-1, -1)))
        {
            Destroy(unitBehaviour.gameObject);
            return;
        }

        // assign block to board
        _board.SetUnitBehaviour(blockCoordinates.Value, unitBehaviour);

        // drop block gameobject into position
        DropBlock(unitBehaviour.gameObject, _board.GetWorldSpacePositionForBoardCoordinates(blockCoordinates.Value));
    }

    private void DropBlock(GameObject blockGameobject, Vector3 position)
    {
        var dropFrom = new Vector3(position.x, Camera.main.orthographicSize + 1, position.z);
        blockGameobject.transform.position = dropFrom;
        
        Drop(blockGameobject, dropFrom, position);
        
    }

    private void Drop(GameObject obj, Vector3 dropFrom, Vector3 dropTo)
    {
        // Initial scale
        var initialScale = obj.transform.localScale;
        obj.transform.localScale = new Vector3(.7f, initialScale.y, initialScale.z);

        // Set a constant falling speed
        float fallSpeed = 15.0f;  // Unity units per second. Adjust as necessary.

        // Calculate the drop duration based on distance to travel and the constant speed
        float dropDistance = Vector3.Distance(dropFrom, dropTo);
        float dropDuration = dropDistance / fallSpeed;

        // Create a sequence for DOTween animations
        Sequence mySequence = DOTween.Sequence();

        // Add move tween to the sequence
        mySequence.Append(obj.transform.DOMove(dropTo, dropDuration).SetEase(Ease.InQuad));

        // Add squash effect once the movement is completed
        mySequence.Append(obj.transform.DOScale(new Vector3(1.5f, 0.6f, initialScale.z), 0.1f));

        // After squashing, spring back to original size
        mySequence.Append(obj.transform.DOScale(initialScale, 0.1f));
    }

    // create the game board where pieces will be populated. whenever the value of numColumns or numRows is changed, the
    // game board should repopulate while in edit mode
    
    // the board should originate from the gameobject from which this script is attached, so it can be dragged around and
    // readjusted while in edit mode
    private void CreateBoard()
    {
        _board = new Board(numColumns, numRows);
        
        // create grid background
        for (int i = 0; i < _board.boardPositions.Length; i++)
        {
            for (int j = 0; j < _board.boardPositions[i].Length; j++)
            {
                var backgroundCell = Instantiate(cellPrefabs[(i + j) % 2]);
                backgroundCell.transform.position = _board.boardPositions[i][j].worldSpacePosition;
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
        
        PrepareTurn();
    }
    
    private Coordinates? FindBlockPlacement()
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
        foreach (var hero in _player.allHeroes)
        {
            var unitBehaviour = CreateBlock(hero);
            Vector3? position = _board.SetHero(unitBehaviour);
            if (position != null)
            {
                DropBlock(unitBehaviour.gameObject, position.Value);
            }
        }
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
    
    public void SwapBlocks(Coordinates leftBlockCoords, Coordinates rightBlockCoords)
    {
        _board.SwapBlocks(leftBlockCoords, rightBlockCoords);
        StartCoroutine(WaitToApplyGravity(_board.blockSwapTime));
    }

    private IEnumerator WaitToApplyGravity(float blockSwapTime)
    {
        yield return new WaitForSeconds(blockSwapTime);
        ApplyGravity();
    }
    
    private void ApplyGravity()
    {
        for (int column = 0; column < numColumns; column++)
        {
            List<UnitBehaviour> collapsedBlocks = new List<UnitBehaviour>();
            for (int row = 0; row < numRows; row++)
            {
                var b = _board.GetUnitBehaviour(new Coordinates(column, row));
                if (b == null || !b.unit) continue;
            
                collapsedBlocks.Add(b);
            }

            for (int newRow = 0; newRow < numRows; newRow++)
            {
                Vector3 newPosition = _board.GetWorldSpacePositionForBoardCoordinates(new Coordinates(column, newRow));
                if (newRow < collapsedBlocks.Count)
                {
                    // Instruct all non-empty blocks to be compressed to the bottom of the board
                    var b = collapsedBlocks[newRow];
                    b.targetPosition = newPosition;
                    _board.SetUnitBehaviour(new Coordinates(column, newRow), b);
                }
                else
                {
                    // Clear any remaining positions above the collapsed blocks
                    _board.SetUnitBehaviour(new Coordinates(column, newRow), null);
                }
            }
        }
    }

    private List<UnitBehaviour> Chain(UnitBehaviour origin)
    {
        return GetNeighboringCoordinates(origin.coordinates).Select((coordinate) 
            => _board.GetUnitBehaviour(coordinate)).Where((block) 
            => block.unit == origin.unit).SelectMany(Chain).ToList();
    }
    
    // Pass in the coordinates of the two blocks the selector is highlighting.
    // This will then return the transform in the middle of the two where the selector gameobject should be rendered
    public Vector3 GetSelectorPosition(Coordinates leftBlockCoords, Coordinates rightBlockCoords)
    {
        var lBlock = _board.GetWorldSpacePositionForBoardCoordinates(leftBlockCoords);
        var rBlock = _board.GetWorldSpacePositionForBoardCoordinates(rightBlockCoords);

        Vector2 middlePos;
        float x = rBlock.x - ((rBlock.x - lBlock.x) / 2);
        float y = rBlock.y - ((rBlock.y - lBlock.y) / 2);
        middlePos = new Vector3(x, y, -50);
        return middlePos;
    }

    public void SetSelectorPosition(Coordinates pos1, Coordinates pos2)
    {
        List<Coordinates> coords = new List<Coordinates>();
        coords.Add(pos1);
        coords.Add(pos2);
    }
    
    public bool IsSelectorColliding(BoardManager.Coordinates block1, BoardManager.Coordinates block2)
    {
        bool isColliding = false;

        foreach (var player in GameManager.Instance.playersInGame)
        {
            if ((block1.Equals(player.selector._leftBlockCoordinates) || (block1.Equals(player.selector._rightBlockCoordinates)) || block2.Equals(player.selector._leftBlockCoordinates) || (block2.Equals(player.selector._rightBlockCoordinates))))
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

    private UnitBehaviour CreateBlock(Unit unit)
    {
        var blockInstance = Instantiate(blockPrefab);

        // hydrate generic block prefab 
        return blockInstance.GetComponent<UnitBehaviour>().Initialize(unit);
    }

    public UnitBehaviour GetBlock(Coordinates coordinates)
    {
        return _board.GetUnitBehaviour(coordinates);
    }

    public void PerformCombat()
    {
        var heroes = _board.Heroes;
        var enemies = _board.FrontRowEnemies;

        // heroes attack first
        for (var i = 0; i < numColumns; i++)
        {
            if (!heroes[i] || !enemies[i]) continue;  // Use continue instead of return

            var hero = heroes[i];
            var enemy = enemies[i];

            var heroPos = hero.transform.position;
            var enemyPos = enemy.transform.position;
        
            hero.transform.DOMove((Vector3) enemyPos, .2f).SetEase(Ease.InExpo).OnComplete(() =>
            {
                enemy.TakeDamage(hero.unit.attack);
                // Move the hero back after damaging the enemy
                hero.transform.DOMove((Vector3) heroPos, .3f).SetEase(Ease.OutQuad);
                
                RemoveDeadUnits();
                
                ApplyGravity();
            });
        }
        
        // then enemies
        for (var i = 0; i < numColumns; i++)
        {
            if (!heroes[i] || !enemies[i]) return;
            
            if (enemies[i].currentHp > 0) enemies[i].TakeDamage(heroes[i].unit.attack);
        }
    }

    private void RemoveDeadUnits()
    {
        UnitBehaviour[] allUnits = _board.GetAllUnits();
        foreach(UnitBehaviour unit in allUnits)
        {
            if(unit.currentHp <= 0)
            {
                _board.RemoveUnitFromBoard(unit);
                Destroy(unit.gameObject);
            }
        }
    }

    private void PrepareTurn()
    {
        var combatParticipants = _board.FrontRowEnemies.ToList();
        foreach (var unitBehaviour in _board.Heroes) combatParticipants.Add(unitBehaviour);
        TurnManager.Instance.ChooseTurnOrder(combatParticipants);
    }
}