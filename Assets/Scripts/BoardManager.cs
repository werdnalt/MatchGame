using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;
using TMPro;
using Random = UnityEngine.Random;
using System.Linq;
using JetBrains.Annotations;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public GameObject blockPrefab;
    public GameObject smokePrefab;
    public GameObject rubbleBlock;
    public GameObject bombPrefab;
    public GameObject deployObject;

    public List<GameObject> cellPrefabs;
    public List<GameObject> heroCellPrefabs;

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

    public UnitBehaviour mostRecentlyAttackingUnit;

    private float blockSize;

    private Player _player;
    private List<GameObject> _objectsToDestroyLater = new List<GameObject>();
    private GameObject[,] cellObjects;
    private int combo = 0;
    private int accumulatedPoints = 0;
    private List<int> accumulatedScores = new List<int>();
    private Dictionary<int, List<Coordinates>> _selectorPositions = new Dictionary<int, List<Coordinates>>();
    private List<UnitBehaviour> _unitsToReinsert = new List<UnitBehaviour>();
    public bool canMove;

    [SerializeField] private GameObject blocksParent;
    [SerializeField] private GameObject cellsParent;
    [SerializeField] private GameObject singleCellSelector;
    
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
        _player = GameObject.FindObjectOfType<Player>();
        
        EventManager.Instance.LevelLoaded();
        canMove = true;

        StartCoroutine(KickOffGameLoop());
    }

    public IEnumerator KickOffGameLoop()
    {
        yield return StartCoroutine(CreateHeroes());

        yield return StartCoroutine(SpawnUnit(WaveManager.Instance.GetUnitsToSpawn()));
        
        StartCoroutine(ModifiedGameLoop());
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
        DropBlock(unitBehaviour.gameObject, _board.GetWorldSpacePositionForBoardCoordinates(blockCoordinates.Value), blockCoordinates.Value);
    }

    private void DropBlock(GameObject blockGameobject, Vector3 position, Coordinates blockCoords)
    {
        var dropFrom = new Vector3(position.x, Camera.main.orthographicSize + 1, position.z);
        blockGameobject.transform.position = dropFrom;

        var newPos = new Vector3(position.x, position.y, blockCoords.y);
        Drop(blockGameobject, dropFrom, newPos);
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
        var cellSize = cellPrefabs[1].GetComponent<SpriteRenderer>().bounds.size;
        _board = new Board(numColumns, numRows, cellSize);
        
        // create grid background
        for (int i = 0; i < _board.boardPositions.Length; i++)
        {
            for (int j = 0; j < _board.boardPositions[i].Length; j++)
            {
                var backgroundCell = Instantiate(cellPrefabs[(i + j) % 2], cellsParent.
                    transform);
                backgroundCell.transform.position = _board.boardPositions[i][j].worldSpacePosition;

                backgroundCell.GetComponent<Cell>().coordinates = new Coordinates(i, j);
            }
        }
        
        // create grid background for heroes
        for (int i = 0; i < _board.heroPositions.Length; i++)
        {
                var backgroundCell = Instantiate(heroCellPrefabs[(i) % 2], cellsParent.
                    transform);
                backgroundCell.transform.position = _board.heroPositions[i].worldSpacePosition;
                Destroy(backgroundCell.GetComponent<Cell>());
                backgroundCell.AddComponent<HeroCell>();

                backgroundCell.GetComponent<HeroCell>().column = i;
        }

        EventManager.Instance.BoardReady();
    }

    public IEnumerator SpawnWave()
    {
        if (!WaveManager.Instance.shouldSpawnWave) yield break;
        
        var unitsToSpawn = WaveManager.Instance.GetUnitsToSpawn();
        yield return StartCoroutine(SpawnUnit(unitsToSpawn));
    }

    public void ForceSpawnWave()
    {
        var unitsToSpawn = WaveManager.Instance.GetUnitsToSpawn();
        StartCoroutine(SpawnUnit(unitsToSpawn));
        TurnManager.Instance.UpdateSwapCounter();
    }

    private IEnumerator SpawnUnit(List<Unit> unitsToSpawn)
    {
        for (var unitsSpawned = 0; unitsSpawned < unitsToSpawn.Count; unitsSpawned++)
        {
            AddBlock(unitsToSpawn[unitsSpawned]);
            yield return new WaitForSeconds(.1f);
        }
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
    
    private IEnumerator CreateHeroes()
    {
        foreach (var hero in _player.allHeroes)
        {
            var unitBehaviour = CreateBlock(hero);
            Vector3? position = _board.SetHero(unitBehaviour);
            if (position != null)
            {
                DropBlock(unitBehaviour.gameObject, position.Value, new Coordinates(0, 0));
            }

            yield return null;
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
        if (!canMove) return;

        if (!EnergyManager.Instance.DoHaveEnoughEnergy(EnergyManager.Instance.energyPerSwap)) return;
        
        EnergyManager.Instance.SpendEnergy(EnergyManager.Instance.energyPerSwap);
        
        var leftUnit = _board.GetUnitBehaviour(leftBlockCoords);
        var rightUnit = _board.GetUnitBehaviour(rightBlockCoords);

        if (leftUnit == null && rightUnit == null) return;
        
        AudioManager.Instance.PlayWithRandomPitch("whoosh");
        TurnManager.Instance.SwapBlocks();
        _board.SwapBlocks(leftBlockCoords, rightBlockCoords);

        if (leftUnit)
        {
            foreach (var effect in leftUnit.effects)
            {
                effect.effect.OnSwap(leftUnit, rightUnit);
            }

        }

        if (rightUnit)
        {
            foreach (var effect in rightUnit.unitData.effects)
            {
                effect.OnSwap(rightUnit, leftUnit);
            }
        }

        StartCoroutine(WaitToApplyGravity(_board.blockSwapTime));
    }

    public IEnumerator WaitToApplyGravity(float blockSwapTime)
    {
        yield return new WaitForSeconds(blockSwapTime);
        CleanUpBoard();
    }

    public void CleanUpBoard()
    {
        RemoveDeadUnits();
        ApplyGravity();
        StartCoroutine(AssignCombatTargets());
    }
    
    private void ApplyGravity()
    {
        for (int column = 0; column < numColumns; column++)
        {
            List<UnitBehaviour> collapsedBlocks = new List<UnitBehaviour>();
            for (int row = 0; row < numRows; row++)
            {
                var b = _board.GetUnitBehaviour(new Coordinates(column, row));
                if (b == null || !b.unitData) continue;
            
                collapsedBlocks.Add(b);
            }

            for (int newRow = 0; newRow < numRows; newRow++)
            {
                var coords = _board.GetWorldSpacePositionForBoardCoordinates(new Coordinates(column, newRow));
                Vector3 newPosition = new Vector3(coords.x, coords.y, coords.y);
                    
                if (newRow < collapsedBlocks.Count)
                {
                    // Instruct all non-empty blocks to be compressed to the bottom of the board
                    var b = collapsedBlocks[newRow];
                    b.targetPosition = newPosition;
                    _board.SetUnitBehaviour(new Coordinates(column, newRow), b);
                    if (newPosition.y < b.transform.position.y - 1) Drop(b.gameObject,b.transform.position, newPosition);
                }
                else
                {
                    // Clear any remaining positions above the collapsed blocks
                    _board.SetUnitBehaviour(new Coordinates(column, newRow), null);
                }
            }
        }
    }

    public List<UnitBehaviour> Chain(UnitBehaviour origin)
    {
        var visited = new HashSet<BoardManager.Coordinates>();
        var allUnits = new List<UnitBehaviour>();

        DFS(origin, visited, allUnits);

        return allUnits;
    }

    private void DFS(UnitBehaviour current, HashSet<BoardManager.Coordinates> visited, List<UnitBehaviour> allUnits)
    {
        if (current == null)
            return;

        if (visited.Contains(current.coordinates))
            return;

        Debug.Log("Made it here");
        visited.Add(current.coordinates);
        allUnits.Add(current);

        foreach (var neighborCoordinates in GetNeighboringCoordinates(current.coordinates))
        {
            var neighbor = _board.GetUnitBehaviour(neighborCoordinates);

            // If neighbor is of the same tribe and hasn't been visited, we recursively call DFS on it.
            if (neighbor != null && neighbor.unitData.tribe == current.unitData.tribe && !visited.Contains(neighbor.coordinates))
            {
                Debug.Log("Same tribe");
                DFS(neighbor, visited, allUnits);
            }
        }
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
    
    public Coordinates GetNeighborCoordinates(Coordinates origin, Direction direction)
    {
        switch (direction)
        {
            case (Direction.Left):
                if (origin.x - 1 >= 0)
                {
                    return new Coordinates(origin.x - 1, origin.y);
                }

                break;
            case (Direction.Right):
                if (origin.x + 1 < numColumns)
                {
                    return new Coordinates(origin.x + 1, origin.y);
                }

                break;
        }

        return new Coordinates(0, 0);
    }
    
    public List<Coordinates> GetNeighboringCoordinates(Coordinates coordinates)
    {
        List<Coordinates> neighbors = new List<Coordinates>();
        int x = coordinates.x;
        int y = coordinates.y;
        
        Debug.Log($"Checking neighbors for coordinates: {coordinates.x}, {coordinates.y} ");

        if (x - 1 >= 0)
        {
            neighbors.Add(new Coordinates(x - 1, y));
        }

        if (y + 1 < numRows)
        {
            neighbors.Add(new Coordinates(x, y + 1));
        }

        if (x + 1 < numColumns)
        {
            neighbors.Add(new Coordinates(x + 1, y));
        }

        if (y - 1 >= 0)
        {
            neighbors.Add(new Coordinates(x, y - 1));
        }
        
        return neighbors;
    }
    
    public List<Coordinates> GetAllNeighboringCoordinates(Coordinates coordinates)
    {
        List<Coordinates> neighbors = new List<Coordinates>();
        int x = coordinates.x;
        int y = coordinates.y;

        Debug.Log($"Checking neighbors for coordinates: {coordinates.x}, {coordinates.y} ");

        // Top neighbor
        if (y + 1 < numRows)
        {
            neighbors.Add(new Coordinates(x, y + 1));
        }

        // Right neighbor
        if (x + 1 < numColumns)
        {
            neighbors.Add(new Coordinates(x + 1, y));
        }

        // Bottom neighbor
        if (y - 1 >= 0)
        {
            neighbors.Add(new Coordinates(x, y - 1));
        }

        // Left neighbor
        if (x - 1 >= 0)
        {
            neighbors.Add(new Coordinates(x - 1, y));
        }

        // Top-left diagonal neighbor
        if (x - 1 >= 0 && y + 1 < numRows)
        {
            neighbors.Add(new Coordinates(x - 1, y + 1));
        }

        // Top-right diagonal neighbor
        if (x + 1 < numColumns && y + 1 < numRows)
        {
            neighbors.Add(new Coordinates(x + 1, y + 1));
        }

        // Bottom-left diagonal neighbor
        if (x - 1 >= 0 && y - 1 >= 0)
        {
            neighbors.Add(new Coordinates(x - 1, y - 1));
        }

        // Bottom-right diagonal neighbor
        if (x + 1 < numColumns && y - 1 >= 0)
        {
            neighbors.Add(new Coordinates(x + 1, y - 1));
        }

        return neighbors;
    }
    
    private UnitBehaviour CreateBlock(Unit unit)
    {
        var blockInstance = Instantiate(blockPrefab, blocksParent.transform);

        // hydrate generic block prefab 
        return blockInstance.GetComponent<UnitBehaviour>().Initialize(unit);
    }

    public UnitBehaviour GetBlock(Coordinates coordinates)
    {
        return _board.GetUnitBehaviour(coordinates);
    }
    
    private IEnumerator SequentialCombat()
    {
        //canMove = false;
        yield return new WaitForSeconds(1f);
        foreach (var unit in TurnManager.Instance.orderedCombatUnits)
        {

            
            if (unit.combatTarget == null) TurnManager.Instance.ResetUnit(unit);
            
            // yield return StartCoroutine(unit.Attack());

            TurnManager.Instance.ResetUnit(unit);
            
            _unitsToReinsert.Add(unit);
            
            CleanUpBoard();

            yield return new WaitForSeconds(.5f);
        }
        
        foreach (var unit in _unitsToReinsert)
        {
            TurnManager.Instance.ReinsertUnit(unit);
        }
        
        _unitsToReinsert.Clear();
    }

    private void RemoveDeadUnits()
    {
        UnitBehaviour[] allUnits = _board.GetAllUnits();
        foreach(UnitBehaviour unit in allUnits)
        {
            if(unit.currentHp <= 0)
            {
                unit.transform.DOKill();
                RemoveUnitFromBoard(unit);
            }
        }
    }

    public UnitBehaviour[] GetAllUnits()
    {
        return _board.GetAllUnits();
    }

    public void RemoveUnitFromBoard(UnitBehaviour unitBehaviour)
    {
        _board.RemoveUnitFromBoard(unitBehaviour);
        Destroy(unitBehaviour.gameObject);
    }

    private IEnumerator AssignCombatTargets()
    {
        for (var i = 0; i < numColumns; i++)
        {
            var hero = _board.Heroes[i];
            var enemy = _board.FrontRowEnemies[i];

            if (enemy)
            {
                hero.SetCombatTarget(enemy);
                enemy.SetCombatTarget(hero);
            }
            else
            {
                hero.SetCombatTarget(null);
            }
        }
        
        yield break;
    }

    private IEnumerator SetUpCombatTurns()
    {
        var combatParticipants = _board.FrontRowEnemies.ToList();
        foreach (var unitBehaviour in _board.Heroes) combatParticipants.Add(unitBehaviour);
        canMove = true;
        return TurnManager.Instance.ChooseTurnOrder(combatParticipants);
    }

    public void SetCellSelector(Vector3 position)
    {
        singleCellSelector.transform.position = position;
    }

    public void SetCellSelector(Coordinates coordinates)
    {
        singleCellSelector.transform.position = _board.GetWorldSpacePositionForBoardCoordinates(coordinates);
    }

    [CanBeNull]
    public UnitBehaviour GetUnitBehaviourAtCoordinate(Coordinates coordinates)
    {
        return _board.GetUnitBehaviour(coordinates);
    }
    
    [CanBeNull]
    public UnitBehaviour GetHeroUnitBehaviourAtCoordinate(int column)
    {
        return _board.GetHeroFromColumn(column);
    }
    
    private IEnumerator GameLoop()
    {
        while (true)
        {
            // spawn wave 
            yield return StartCoroutine(SpawnWave());

            // choose combat targets for heroes and enemies
            yield return StartCoroutine(AssignCombatTargets());
        
            // assign combatants their turn order and create turn UIs
            yield return StartCoroutine(SetUpCombatTurns());
            
            // player swaps blocks
            yield return StartCoroutine(TurnManager.Instance.CheckIfFinishedSwapping());
            
            // combat happens
            yield return StartCoroutine(SequentialCombat());
        }
    }
    
    
    private IEnumerator ModifiedGameLoop()
    {
        while (true)
        {
            // assign combat targets
            yield return StartCoroutine(AssignCombatTargets());
            
            // wait for player to swap blocks
            yield return StartCoroutine(TurnManager.Instance.CheckIfFinishedSwapping());
        }
    }

    public Coordinates? GetCoordinatesForUnit(UnitBehaviour unitBehaviour)
    {
        return _board.FindUnitBehaviour(unitBehaviour);
    }

    public bool IsNeighbor(Cell cell1, Cell cell2)
    {
        var coordinates1 = cell1.coordinates;
        var coordinates2 = cell2.coordinates;

        if (coordinates1.y != coordinates2.y) return false;
        
        if (Mathf.Abs(coordinates1.x - coordinates2.x) == 1)
        {
            return true;
        }

        return false;
    }

    public bool IsHeroNeighbor(Cell cell1, Cell cell2)
    {
        if (Mathf.Abs(cell1.column - cell2.column) == 1)
        {
            return true;
        }

        return false;
    }

    public bool CanAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit)
    {
        var combatTarget = attackingUnit.combatTarget;

        if (Mathf.Abs(attackedUnit.coordinates.x - combatTarget.coordinates.x) <= attackingUnit.unitData.attackRange && 
            Mathf.Abs(attackedUnit.coordinates.y - combatTarget.coordinates.y) <= attackingUnit.unitData.attackRange)
        {
            return true;
        }

        return false;
    }
}