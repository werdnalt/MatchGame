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
using UnityEditor.PackageManager;
using UnityEngine.SceneManagement;

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

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    public int numRows;
    public int numColumns;
    public float initialBlockDropSpeed = .02f;
    
    private float _blockSize;
    private int _enemyFrontRow = 1;

    private Dictionary<Coordinates, Cell> _cells;
    
    [SerializeField] private UnitBehaviour unitBehaviourPrefab;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private List<Cell> heroCellPrefabs;
    [SerializeField] private GameObject blocksParent;
    [SerializeField] private GameObject cellsParent;
    [SerializeField] private GameObject singleCellSelector;
    
    public enum Direction
    {
        Up, 
        Down,
        Left,
        Right
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public List<UnitBehaviour> Heroes
    {
        get
        {
            var heroes = new List<UnitBehaviour>();
            for (var i = 0; i < numColumns; i++)
            {
                var hero = GetUnitBehaviour(new Coordinates(i, Timings.HeroRow));
                if (hero) heroes.Add(hero);
            }

            return heroes;
        }
    }
    
    public List<UnitBehaviour> FrontRowEnemies
    {
        get
        {
            var frontRowEnemies = new List<UnitBehaviour>();
            for (var i = 0; i < numColumns; i++)
            {
                var enemy = GetUnitBehaviour(new Coordinates(i, Timings.EnemyRow));
                if (enemy) frontRowEnemies.Add(enemy);
            }

            return frontRowEnemies;
        }
    }
    
    public IEnumerator InitializeBoard()
    {
        yield return StartCoroutine(CreateBoard()); // creates the actual grid composed of square gameobjects. unit placement will be based on these transforms
        
        yield return StartCoroutine(CreateHeroes());

        yield return StartCoroutine(SpawnEnemyUnits(WaveManager.Instance.GetUnitsToSpawn()));
        
        StartCoroutine(ModifiedGameLoop());
    }

    /// <summary>
    /// Creates a UnitBehaviour from provided Unit Data.
    /// Places the block in the appropriate location on the board
    /// </summary>
    public UnitBehaviour SpawnUnit(Unit unit, int lowestRow)
    {
        // create unit behavior for the unit
        var unitBehaviour = CreateUnitBehaviour(unit);

        // find available board position on grid for unit
        var blockCoordinates = FindRandomColumn(lowestRow);
        
        if (blockCoordinates == null || blockCoordinates.Equals(new Coordinates(-1, -1)))
        {
            unitBehaviour.RemoveSelf();
            return null;
        }

        SetUnitBehaviour(unitBehaviour, blockCoordinates.Value);

        // drop block gameobject into position
        Drop(unitBehaviour.gameObject, GetPositionFromCoordinates(blockCoordinates.Value).Value);

        return unitBehaviour;
    } 

    private void Drop(GameObject blockGameObject, Vector3 position)
    {
        var dropFrom = new Vector3(position.x, Camera.main.orthographicSize + 1, position.z);
        blockGameObject.transform.position = dropFrom;

        var dropTo = new Vector3(position.x, position.y, 0);
        
        // Initial scale
        var initialScale = blockGameObject.transform.localScale;
        //obj.transform.localScale = new Vector3(initialScale.x * .7f, initialScale.y, initialScale.z);

        // Set a constant falling speed
        float fallSpeed = 15.0f;  // Unity units per second. Adjust as necessary.

        // Calculate the drop duration based on distance to travel and the constant speed
        float dropDistance = Vector3.Distance(dropFrom, dropTo);
        float dropDuration = dropDistance / fallSpeed;

        // Create a sequence for DOTween animations
        Sequence mySequence = DOTween.Sequence();

        // Add move tween to the sequence
        mySequence.Append(blockGameObject.transform.DOMove(dropTo, dropDuration).SetEase(Ease.InQuad));

        // Add squash effect once the movement is completed
        mySequence.Append(blockGameObject.transform.DOScale(new Vector3(1.5f, 0.6f, initialScale.z), 0.1f));

        // After squashing, spring back to original size
        mySequence.Append(blockGameObject.transform.DOScale(initialScale, 0.1f));
    }

    private Vector3? GetPositionFromCoordinates(Coordinates coordinates)
    {
        if (_cells.TryGetValue(coordinates, out var cell))
        {
            return cell.boardPosition;
        }

        return null;
    }
    
    // create the game board where pieces will be populated. whenever the value of numColumns or numRows is changed, the
    // game board should repopulate while in edit mode
    
    // the board should originate from the gameobject from which this script is attached, so it can be dragged around and
    // readjusted while in edit mode
    private IEnumerator CreateBoard()
    {
        var boardReady = false;
        var cellInstances = new List<Cell>();

        // create Cells
        for (var i = 0; i < numColumns; i++)
        {
            for (var j = 0; j < numRows; j++)
            {
                var cellInstance = Instantiate(cellPrefab, cellsParent.transform);
                cellInstance.Coordinates = new Coordinates(i, j);
                cellInstance.boardPosition = GetCellPosition(cellInstance);
                
                cellInstances.Add(cellInstance);
                _cells.Add(cellInstance.Coordinates, cellInstance);
            }
        }

        // Shuffle the blocksToDrop list
        var rng = new System.Random();
        var n = cellInstances.Count;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            (cellInstances[k], cellInstances[n]) = (cellInstances[n], cellInstances[k]);
        }

        // Drop blocks in shuffled order
        foreach (var cell in cellInstances)
        {
            Drop(cell.gameObject, cell.boardPosition);
            yield return new WaitForSeconds(initialBlockDropSpeed);
        }
        
        boardReady = true;
        yield return new WaitUntil(() => boardReady);
        EventManager.Instance.BoardReady();
    }
    
    /// <summary>
    /// Returns the World Position where a cell should be placed based on the number of total cells and the cell size.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    private Vector3 GetCellPosition(Cell cell)
    {
        var yOffset = .5f;
        var prefabSize = cellPrefab.GetComponent<SpriteRenderer>().bounds.size;
        
        // Calculate the total width and height of the board based on prefab size
        float boardWidth = numColumns * prefabSize.x; 
        float boardHeight = (numRows - 1) * prefabSize.x;

        // Calculate the starting position for the board to be centered on the screen
        float startX = -boardWidth / 2 + prefabSize.x / 2; 
        float startY = (-boardHeight / 2) + prefabSize.x / 2 + cell.Coordinates.y * prefabSize.y + yOffset;  

        float xPos = startX + cell.Coordinates.x * prefabSize.x;
        return new Vector3(xPos, startY, 0f); // Assuming Z value is 0, adjust if needed
    }

    public IEnumerator SpawnWave()
    {
        if (!WaveManager.Instance.shouldSpawnWave) yield break;
        
        var unitsToSpawn = WaveManager.Instance.GetUnitsToSpawn();
        yield return StartCoroutine(SpawnEnemyUnits(unitsToSpawn));
    }

    public void ForceSpawnWave()
    {
        var energyCost = 3;
        
        if (!EnergyManager.Instance.DoHaveEnoughEnergy(energyCost)) return;
        
        EnergyManager.Instance.SpendEnergy(energyCost);
        var unitsToSpawn = WaveManager.Instance.GetUnitsToSpawn();
        if (unitsToSpawn == null) return;
        StartCoroutine(SpawnEnemyUnits(unitsToSpawn));
    }

    private IEnumerator SpawnEnemyUnits(List<Unit> unitsToSpawn)
    {
        for (var unitsSpawned = 0; unitsSpawned < unitsToSpawn.Count; unitsSpawned++)
        {
            SpawnUnit(unitsToSpawn[unitsSpawned], Timings.EnemyRow);
            yield return new WaitForSeconds(.1f);
        }
    }
    
    /// <summary>
    /// Finds the coordinates of the first available position in a random column
    /// </summary>
    /// <returns>
    /// The coordinate of the position. If board is full, returns null
    /// </returns>
    private Coordinates? FindRandomColumn(int lowestRow)
    {
        List<int> availableColumnIndices = new List<int>();

        // find any columns that have at least one available cell
        for (int column = 0; column < numColumns; column++)
        {
            for (int row = lowestRow; row < +numRows; row++)
            {
                if (GetUnitBehaviour(new Coordinates(column, row)) == null)
                {
                    availableColumnIndices.Add(column);
                    break;  // Exit the inner loop once we find an available cell in the column.
                }
            }
        }

        // If there are no available columns, handle this case.
        if (availableColumnIndices.Count == 0)
        {
            // Depending on your requirements, return a default value or handle differently.
            return new Coordinates(-1, -1);
        }

        // pick a random column from the available columns
        int chosenColumnIndex = availableColumnIndices[Random.Range(0, availableColumnIndices.Count)];

        // Find the first available cell in the chosen column.
        Coordinates? cellCoordinates = FindFirstAvailablePositionInColumn(chosenColumnIndex);

        return cellCoordinates;
    }
    
    private Coordinates? FindFirstAvailablePositionInColumn(int columnIndex)
    {
        for (var row = 0; row < numRows; row++)
        {
            if (GetUnitBehaviour(new Coordinates(columnIndex, row)) == null)
            {
                return new Coordinates(columnIndex, row);
            }
        }

        return null;
    }

    public void SwapUnits(Coordinates leftBlockCoords, Coordinates rightBlockCoords)
    {
        if (!GamePlayDirector.Instance.playerActionPermitted) return;

        var leftUnit = GetUnitBehaviour(leftBlockCoords);
        var rightUnit = GetUnitBehaviour(rightBlockCoords);
        if (leftUnit == null && rightUnit == null) return;
        
        SwapUnitsOnBoard(leftBlockCoords, rightBlockCoords);
        
        EventPipe.TakeAction();
        AudioManager.Instance.PlayWithRandomPitch("whoosh");

        if (leftUnit && leftUnit.effects.Count > 0)
        {
            var leftEffectsCopy = new List<EffectState>(leftUnit.effects);  // Create a copy
            foreach (var effectState in leftEffectsCopy)
            {
                effectState.effect.OnSwap(leftBlockCoords, rightBlockCoords);
            }
        }

        if (rightUnit && rightUnit.effects.Count > 0)
        {
            var rightEffectsCopy = new List<EffectState>(rightUnit.effects);  // Create a copy
            foreach (var effectState in rightEffectsCopy)
            {
                effectState.effect.OnSwap(rightBlockCoords, leftBlockCoords);
            }
        }

        StartCoroutine(WaitToApplyGravity(_board.blockSwapTime));
    }



    private void SwapUnitsOnBoard(Coordinates cell1, Coordinates cell2)
    {
        var unitBehaviour1 = GetUnitBehaviour(cell1);
        var unitBehaviour2 = GetUnitBehaviour(cell2);

        SetUnitBehaviour(unitBehaviour2, cell1);
        SetUnitBehaviour(unitBehaviour1, cell2);
    }
    
    private IEnumerator WaitToApplyGravity(float blockSwapTime)
    {
        yield return new WaitForSeconds(blockSwapTime);
        CleanUpBoard();
    }

    public void CleanUpBoard()
    {
        RemoveDeadUnits();
        ApplyGravity();
    }
    
    private void ApplyGravity()
    {
        for (var column = 0; column < numColumns; column++)
        {
            var collapsedBlocks = new List<UnitBehaviour>();
            for (var row = 0; row < numRows; row++)
            {
                var unitBehaviour = GetUnitBehaviour(new Coordinates(column, row));
                if (unitBehaviour == null || !unitBehaviour.unitData) continue;
            
                collapsedBlocks.Add(unitBehaviour);
            }

            for (var newRow = 0; newRow < numRows; newRow++)
            {
                var collapsedCoordinates = new Coordinates(column, newRow);
                var cellPosition = GetCell(collapsedCoordinates).boardPosition;
                    
                if (newRow < collapsedBlocks.Count)
                {
                    // Instruct all non-empty blocks to be compressed to the bottom of the board
                    var unitBehaviour = collapsedBlocks[newRow];
                    unitBehaviour.targetPosition = cellPosition;
                    SetUnitBehaviour(unitBehaviour, collapsedCoordinates);
                    
                    if (collapsedCoordinates.y < unitBehaviour.transform.position.y - 1) Drop(unitBehaviour.gameObject, unitBehaviour.transform.position);
                    if (newRow == Timings.EnemyRow) unitBehaviour.EnableCountdownTimer();
                }
                else
                {
                    SetUnitBehaviour(null, collapsedCoordinates);
                }
            }
        }
    }

    public List<UnitBehaviour> GetChainedUnits(UnitBehaviour origin)
    {
        var visited = new HashSet<Coordinates>();
        var allUnits = new List<UnitBehaviour>();

        DFS(origin, visited, allUnits);

        return allUnits;
    }

    private void DFS(UnitBehaviour current, HashSet<Coordinates> visited, List<UnitBehaviour> allUnits)
    {
        if (current == null)
            return;

        if (visited.Contains(current.coordinates))
            return;
        
        visited.Add(current.coordinates);
        allUnits.Add(current);

        foreach (var neighborCoordinates in GetCardinalNeighborCoordinates(current.coordinates))
        {
            var neighbor = GetUnitBehaviour(neighborCoordinates);

            // If neighbor is of the same tribe and hasn't been visited, we recursively call DFS on it.
            if (neighbor == null || neighbor.unitData.tribe != current.unitData.tribe ||
                visited.Contains(neighbor.coordinates) || neighbor.cantChain) continue;

            DFS(neighbor, visited, allUnits);
        }
    }
    
    public Coordinates GetSingleNeighborCoordinates(Coordinates origin, Direction direction)
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
    
    public void SetCellSelector(Vector3 position)
    {
        singleCellSelector.transform.position = position;
    }
    
    private IEnumerator ModifiedGameLoop()
    {
        while (true)
        {
            // spawn wave 
            yield return StartCoroutine(SpawnWave());
            
            // assign combat targets
            yield return StartCoroutine(AssignCombatTargets());
            
            yield return StartCoroutine(SetEnemyAttackOrder());
            
            yield return StartCoroutine(TurnManager.Instance.WaitForPlayerTurn());
            
            if (UIManager.Instance.chestDestroyed)
            {
                yield return StartCoroutine(UIManager.Instance.ChestEvent());
            }

            yield return StartCoroutine(SequentialCombat());
        }
    }
    
    public void OnRestart()
    {
        SceneManager.LoadScene("PlayScene");
    }

    #region Neighbor Helpers

    public bool IsNeighbor(Cell cell1, Cell cell2)
    {
        var coordinates1 = cell1.Coordinates;
        var coordinates2 = cell2.Coordinates;

        if (coordinates1.y != coordinates2.y) return false;
        
        if (Mathf.Abs(coordinates1.x - coordinates2.x) == 1)
        {
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Returns the Coordinates of cells to the north, south, east, and west of the cell at the provided coordinates
    /// </summary>
    /// <param name="coordinates">The coordinates of the cell whose neighbors will be returned</param>
    /// <returns></returns>
    public List<Coordinates> GetCardinalNeighborCoordinates(Coordinates coordinates)
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
    

    #endregion

    #region Board Interaction
    
    /// <summary>
    /// Sets the UnitBehaviour to be placed on the board at a given position
    /// Assigns the UnitBehaviour to a given Cell
    /// </summary>
    /// <param name="unitBehaviour"></param>
    /// <param name="cellCoordinates"></param>
    private void SetUnitBehaviour(UnitBehaviour unitBehaviour, Coordinates cellCoordinates)
    {
        var cell = _cells[cellCoordinates];
        if (cell == null)
        {
            Debug.LogError($"Cell at {cellCoordinates.x}, {cellCoordinates.y} not found");
        }

        cell.UnitBehaviour = unitBehaviour;
    }

    [CanBeNull]
    public UnitBehaviour GetUnitBehaviour(Coordinates coordinates)
    {
        return _cells[coordinates].UnitBehaviour;
    }

    [CanBeNull]
    private Coordinates? GetCoordinatesForUnitBehaviour(UnitBehaviour unitBehaviour)
    {
        foreach (var kvp in _cells)
        {
            if (kvp.Value.UnitBehaviour == unitBehaviour)
            {
                return kvp.Key;
            }
        }
        return null;
    }
    
    public void RemoveUnitBehaviour(UnitBehaviour unitBehaviour)
    {
        var unitCoordinates = GetCoordinatesForUnitBehaviour(unitBehaviour);
        if (unitCoordinates == null) return;

        _cells.Remove((Coordinates) unitCoordinates);
        unitBehaviour.RemoveSelf();
    }
    
    /// <summary>
    /// Instantiates a Block prefab from provided Unit data
    /// </summary>
    private UnitBehaviour CreateUnitBehaviour(Unit unit)
    {
        var unitBehaviourInstance = Instantiate(unitBehaviourPrefab, blocksParent.transform);

        // hydrate generic block prefab 
        return unitBehaviourInstance.Initialize(unit);
    }
    
    private void RemoveDeadUnits()
    {
        var deadUnitsKeys = _cells
            .Where(kvp => kvp.Value.UnitBehaviour.currentHp <= 0)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in deadUnitsKeys)
        {
            var cell = _cells[key];
            _cells.Remove(key);
            cell.UnitBehaviour.RemoveSelf();
        }
    }
    
    public List<UnitBehaviour> GetAllUnitBehaviours()
    {
        var allCells = _cells.Values.ToList();
        var allUnitBehaviours = new List<UnitBehaviour>();
        foreach (var cell in allCells)
        {
            allUnitBehaviours.Add(cell.UnitBehaviour);
        }

        return allUnitBehaviours;
    }

    private Cell GetCell(Coordinates coordinates)
    {
        return _cells[coordinates];
    }

    #endregion
}


