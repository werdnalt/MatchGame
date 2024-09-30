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
    public int column;

    public int row;

    public Coordinates(int column, int row)
    {
        this.column = column;
        this.row = row;
    }

    public bool Equals(Coordinates other)
    {
        if (other.column == column && other.row == row)
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

    private Dictionary<Coordinates, Cell> _cells = new Dictionary<Coordinates, Cell>();
    
    [SerializeField] private UnitBehaviour heroUnitBehaviourPrefab;
    [SerializeField] private UnitBehaviour enemyUnitBehaviourPrefab;

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

    public List<UnitBehaviour> HeroesByPosition
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
    
    public List<EnemyUnitBehaviour> FrontRowEnemies
    {
        get
        {
            var frontRowEnemies = new List<EnemyUnitBehaviour>();
            for (var i = 0; i < numColumns; i++)
            {
                var enemy = GetUnitBehaviour(new Coordinates(i, Timings.EnemyRow));
                if (enemy) frontRowEnemies.Add(enemy as EnemyUnitBehaviour);
            }

            return frontRowEnemies;
        }
    }
    
    /// <summary>
    /// Creates a UnitBehaviour from provided Unit Data.
    /// Places the block in the appropriate location on the board
    /// </summary>
    public IEnumerator SpawnUnit(Unit unit, int lowestRow)
    {
        // create unit behavior for the unit
        var unitBehaviour = CreateUnitBehaviour(unit);

        // find available board position on grid for unit
        var blockCoordinates = FindRandomColumn(lowestRow);
        
        if (blockCoordinates == null || blockCoordinates.Equals(new Coordinates(-1, -1)))
        {
            unitBehaviour.RemoveSelf();
            yield break;
        }

        yield return StartCoroutine(SetSpawnedUnitPosition(unitBehaviour, blockCoordinates.Value));
    } 
    
    public void SpawnUnit(Unit unit, int column, int row)
    {
        var unitBehaviour = CreateUnitBehaviour(unit);
        var coordinates = new Coordinates(column, row);
        StartCoroutine(UpdateUnitPosition(unitBehaviour, coordinates));
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
    public IEnumerator CreateBoard()
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
                cellInstance.name = $"{i}, {j}";
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
    public Vector3 GetCellPosition(Cell cell)
    {
        var yOffset = .5f;
        var prefabSize = cellPrefab.GetComponent<SpriteRenderer>().bounds.size;
        
        // Calculate the total width and height of the board based on prefab size
        float boardWidth = numColumns * prefabSize.x; 
        float boardHeight = (numRows - 1) * prefabSize.x;

        // Calculate the starting position for the board to be centered on the screen
        float startX = -boardWidth / 2 + prefabSize.x / 2; 
        float startY = (-boardHeight / 2) + prefabSize.x / 2 + cell.Coordinates.row * prefabSize.y + yOffset;  

        float xPos = startX + cell.Coordinates.column * prefabSize.x;
        return new Vector3(xPos, startY, 0f); // Assuming Z value is 0, adjust if needed
    }
    
    public Vector3 GetCellPosition(Coordinates coordinates)
    {
        var cell = GetCell(coordinates);
        
        var yOffset = .5f;
        var prefabSize = cellPrefab.GetComponent<SpriteRenderer>().bounds.size;
        
        // Calculate the total width and height of the board based on prefab size
        float boardWidth = numColumns * prefabSize.x; 
        float boardHeight = (numRows - 1) * prefabSize.x;

        // Calculate the starting position for the board to be centered on the screen
        float startX = -boardWidth / 2 + prefabSize.x / 2; 
        float startY = (-boardHeight / 2) + prefabSize.x / 2 + cell.Coordinates.row * prefabSize.y + yOffset;  

        float xPos = startX + cell.Coordinates.column * prefabSize.x;
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
        var unitsToSpawn = WaveManager.Instance.GetUnitsToSpawn();
        if (unitsToSpawn == null) return;
        StartCoroutine(SpawnEnemyUnits(unitsToSpawn));
    }

    public IEnumerator SpawnEnemyUnits(List<Unit> unitsToSpawn)
    {
        for (var unitsSpawned = 0; unitsSpawned < unitsToSpawn.Count; unitsSpawned++)
        {
            yield return SpawnUnit(unitsToSpawn[unitsSpawned], Timings.EnemyRow);
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
            for (int row = lowestRow; row < numRows; row++)
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

    public IEnumerator SwapUnits(Coordinates leftBlockCoords, Coordinates rightBlockCoords)
    {
        if (!GamePlayDirector.Instance.PlayerActionAllowed) yield break;

        var leftUnit = GetUnitBehaviour(leftBlockCoords);
        var rightUnit = GetUnitBehaviour(rightBlockCoords);
        if (leftUnit == null && rightUnit == null) yield break;
        
        SwapUnitData(leftBlockCoords, rightBlockCoords);
        
        EventPipe.TakeAction();
        AudioManager.Instance.PlayWithRandomPitch("whoosh");

        // Execute On Swap effects
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

        yield return new WaitForSeconds(Timings.SwapTime);
        
        if (leftUnit) TryToAssignToLowerRow(leftUnit);
        if (rightUnit) TryToAssignToLowerRow(rightUnit);
        yield return new WaitForSeconds(Timings.SwapTime);
    }

    private void TryToAssignToLowerRow(UnitBehaviour unitBehaviour)
    {
        if (unitBehaviour == null) return;

        var currentCell = GetCellFromUnitBehaviour(unitBehaviour);
        if (!currentCell) return;

        for (var i = 0; i < currentCell.Coordinates.row; i++)
        {
            var lowerCoordinates = new Coordinates(currentCell.Coordinates.column, i);
            var unit = GetUnitBehaviour(lowerCoordinates);
        
            // Check if the lower cell is empty
            if (unit == null)
            {
                var oldCell = GetCell(unitBehaviour.currentCoordinates);
                if (oldCell)
                {
                    // Reserve the lower cell first to prevent race conditions
                    var lowerCell = GetCell(lowerCoordinates);
                    if (lowerCell != null)
                    {
                        lowerCell.UnitBehaviour = unitBehaviour;
                        oldCell.UnitBehaviour = null; // Only unassign after reserving the new cell
                    }
                }

                if (lowerCoordinates.Equals(new Coordinates(0, 0))) return;
                // Start the coroutine to move the unit visually
                StartCoroutine(UpdateUnitPosition(unitBehaviour, lowerCoordinates));
                return;
            }
        }
    }



    private void SwapUnitData(Coordinates cell1, Coordinates cell2)
    {
        var unitBehaviour1 = GetUnitBehaviour(cell1);
        var unitBehaviour2 = GetUnitBehaviour(cell2);

        StartCoroutine(UpdateUnitPosition(unitBehaviour2, cell1));
        StartCoroutine(UpdateUnitPosition(unitBehaviour1, cell2));
    }
    
    public IEnumerator WaitToApplyGravity(float blockSwapTime)
    {
        yield return new WaitForSeconds(blockSwapTime);
        
        CleanUpBoard();
    }

    public void CleanUpBoard()
    {
        RemoveDeadUnits();
        //ApplyGravity();
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

        var coordinates = GetCoordinatesForUnitBehaviour(current);
        if (coordinates == null) return;
        
        if (visited.Contains(coordinates.Value))
            return;
        
        visited.Add(coordinates.Value);
        allUnits.Add(current);

        foreach (var neighborCoordinates in GetCardinalNeighborCoordinates(coordinates.Value))
        {
            var neighbor = GetUnitBehaviour(neighborCoordinates);

            // If neighbor is of the same tribe and hasn't been visited, we recursively call DFS on it.
            if (neighbor == null || neighbor.unitData.tribe != current.unitData.tribe ||
                visited.Contains(neighborCoordinates) || neighbor.cantChain) continue;

            DFS(neighbor, visited, allUnits);
        }
    }
    
    public Coordinates GetSingleNeighborCoordinates(Coordinates origin, Direction direction)
    {
        switch (direction)
        {
            case (Direction.Left):
                if (origin.column - 1 >= 0)
                {
                    return new Coordinates(origin.column - 1, origin.row);
                }

                break;
            
            case (Direction.Right):
                if (origin.column + 1 < numColumns)
                {
                    return new Coordinates(origin.column + 1, origin.row);
                }

                break;
        }

        return new Coordinates(0, 0);
    }

    public List<UnitBehaviour> GetAllNeighborUnitBehaviours(UnitBehaviour unitBehaviour)
    {
        var unitCoordinates = GetCoordinatesForUnitBehaviour(unitBehaviour);
        var allNeighbors = new List<UnitBehaviour>();
        if (unitCoordinates != null)
        {
            var allNeighborCoordinates = GetAllNeighboringCoordinates(unitCoordinates.Value);
            
            foreach (var neighborCoordinate in allNeighborCoordinates)
            {
                var neighborUnitBehaviour = GetUnitBehaviour(neighborCoordinate);
                allNeighbors.Add(neighborUnitBehaviour);
            }
        }
        return allNeighbors;
    }
    
    public void SetCellSelector(Coordinates coordinates)
    {
        var cell = GetCell(coordinates);
        singleCellSelector.transform.position = cell.transform.position;
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

        if (coordinates1.row != coordinates2.row) return false;
        
        if (Mathf.Abs(coordinates1.column - coordinates2.column) == 1)
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
        int x = coordinates.column;
        int y = coordinates.row;
        
        Debug.Log($"Checking neighbors for coordinates: {coordinates.column}, {coordinates.row} ");

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
        int x = coordinates.column;
        int y = coordinates.row;

        Debug.Log($"Checking neighbors for coordinates: {coordinates.column}, {coordinates.row} ");

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

    private void MoveUnitBehaviour(UnitBehaviour unitBehaviour,Vector3 newPos)
    {
        if (!unitBehaviour) return;

            // Create a sequence for DOTween animations
            Sequence mySequence = DOTween.Sequence();

            // Add move tween to the sequence
            mySequence.Append(unitBehaviour.transform.DOMove(newPos, Timings.SwapTime).SetEase(Ease.InQuad));

            // Add squash effect once the movement is completed
            mySequence.Append(unitBehaviour.transform.DOScale(new Vector3(1.5f, 0.6f, unitBehaviour.originalScale.z), 0.1f));

            // After squashing, spring back to original size
            mySequence.Append(unitBehaviour.transform.DOScale(unitBehaviour.originalScale, 0.1f));
    }
    

    #endregion

    #region Board Interaction
    
    /// <summary>
    /// Sets the UnitBehaviour to be placed on the board at a given position
    /// Assigns the UnitBehaviour to a given Cell
    /// </summary>
    /// <param name="unitBehaviour"></param>
    /// <param name="cellCoordinates"></param>
    private IEnumerator AssignUnitBehaviourToCell(UnitBehaviour unitBehaviour, Coordinates cellCoordinates)
    {
        var cell = _cells[cellCoordinates];
        if (cell == null)
        {
            Debug.LogError($"Cell at {cellCoordinates.column}, {cellCoordinates.row} not found");
            yield break;
        }
        
        // Assign unit to the cell
        cell.UnitBehaviour = unitBehaviour;

        if (unitBehaviour == null) yield break;

        // Update the unit's coordinates and sorting order
        unitBehaviour.currentCoordinates = cellCoordinates;
        unitBehaviour.ResetSortingOrder();

        // Since this is synchronous, no need for a WaitUntil here
        yield return null; // Yielding to keep coroutine structure
    }
    
    [CanBeNull]
    public UnitBehaviour GetUnitBehaviour(Coordinates coordinates)
    {
        return _cells.TryGetValue(coordinates, out var cell) ? cell.UnitBehaviour : null;
    }

    [CanBeNull]
    public Coordinates? GetCoordinatesForUnitBehaviour(UnitBehaviour unitBehaviour)
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

    private IEnumerator SetSpawnedUnitPosition(UnitBehaviour unit, Coordinates newCoordinates)
    {
        yield return AssignUnitBehaviourToCell(unit, newCoordinates);
        Drop(unit.gameObject, GetCellPosition(newCoordinates));
    }
    
    private IEnumerator UpdateUnitPosition(UnitBehaviour unit, Coordinates newCoordinates)
    {
        yield return AssignUnitBehaviourToCell(unit, newCoordinates);
        MoveUnitBehaviour(unit, GetPositionFromCoordinates(newCoordinates).Value);
    }
    
    public void RemoveUnitBehaviour(UnitBehaviour unitBehaviour)
    {
        var unitCoordinates = GetCoordinatesForUnitBehaviour(unitBehaviour);
        if (unitCoordinates == null) return;

        _cells[(Coordinates) unitCoordinates].UnitBehaviour = null;
        unitBehaviour.RemoveSelf();
    }
    
    /// <summary>
    /// Instantiates a Block prefab from provided Unit data
    /// </summary>
    private UnitBehaviour CreateUnitBehaviour(Unit unit)
    {
        UnitBehaviour unitBehaviourInstance;
        if (unit.tribe == Unit.Tribe.Hero)
        {
            unitBehaviourInstance = Instantiate(heroUnitBehaviourPrefab, blocksParent.transform);
        }
        else
        {
            unitBehaviourInstance = Instantiate(enemyUnitBehaviourPrefab, blocksParent.transform);
        }
        
        return unitBehaviourInstance.Initialize(unit);
    }
    
    private void RemoveDeadUnits()
    {
        var allUnitBehaviours = GetAllUnitBehaviours();
        foreach (var unitBehaviour in allUnitBehaviours)
        {
            if (unitBehaviour.isDead)
            {
                var coordinates = GetCoordinatesForUnitBehaviour(unitBehaviour).Value;
                if (_cells.ContainsKey(coordinates)) _cells[coordinates].UnitBehaviour = null;
                unitBehaviour.RemoveSelf();
            }
        }

        allUnitBehaviours = GetAllUnitBehaviours();
        for (var column = 0; column < numColumns; column++)
        {
            for (var row = 0; row < numRows; row++)
            {
                var unitBehaviour = GetUnitBehaviour(new Coordinates(column, row));
                if (unitBehaviour) TryToAssignToLowerRow(unitBehaviour);
            }
        }
    }
    
    public List<UnitBehaviour> GetAllUnitBehaviours()
    {
        var allCells = _cells.Values.ToList();
        var allUnitBehaviours = new List<UnitBehaviour>();
        foreach (var cell in allCells)
        {
            if (cell.UnitBehaviour != null) allUnitBehaviours.Add(cell.UnitBehaviour);
        }

        return allUnitBehaviours;
    }

    private Cell GetCell(Coordinates coordinates)
    {
        return _cells[coordinates];
    }

    public List<Cell> GetAllCells()
    {
        return _cells.Values.ToList();
    }
    
    private Cell GetCellFromUnitBehaviour(UnitBehaviour unitBehaviour)
    {
        foreach (var kvp in _cells)
        {
            Cell cell = kvp.Value;
            if (cell.UnitBehaviour == unitBehaviour)
            {
                return cell;
            }
        }
        
        return null;
    }

    #endregion
}


