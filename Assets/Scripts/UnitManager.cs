using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;
    
    private Player _player;

    [SerializeField] private BoardManager boardManager;

    private void Awake()
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

    public IEnumerator HandlePlayerActionTaken(int actions)
    {
        var allCells = boardManager.GetAllCells();
        foreach (var cell in allCells)
        {
            // Check if there is a UnitBehaviour at the cell
            if (!cell.UnitBehaviour) continue;
            
            cell.UnitBehaviour.CountdownStatuses(actions);
            
            // Check if the UnitBehaviour's attack range would put them in range of the Hero Row
            if (cell.UnitBehaviour is EnemyUnitBehaviour && cell.UnitBehaviour.attackRange >= cell.Coordinates.row)
            {
                // Show their attack timer UI and count it down
                var enemyUnitBehaviour = cell.UnitBehaviour as EnemyUnitBehaviour;
                if (enemyUnitBehaviour) yield return StartCoroutine(enemyUnitBehaviour.HandleActionTaken(actions));
            }
        }

        foreach (var cell in allCells)
        {
            if (!cell.UnitBehaviour || cell.UnitBehaviour is HeroUnitBehaviour) continue;

            var enemyUnitBehaviour = cell.UnitBehaviour as EnemyUnitBehaviour;
            if (enemyUnitBehaviour == null || !enemyUnitBehaviour.ShouldAttack()) continue;
            
            Debug.Log($"Enemy is ready to attack");
            
            var hero = boardManager.GetUnitBehaviour(new Coordinates(enemyUnitBehaviour.currentCoordinates.column, Timings.HeroRow));
            if (hero)
            {
                yield return StartCoroutine(enemyUnitBehaviour.Attack(hero));
            }
        }
    }

    private void Start()
    {
        _player = Player.Instance;
    }
    
    public IEnumerator CreateHeroes()
    {
        for (var i = 0; i < boardManager.numColumns; i++)
        {
            boardManager.SpawnUnit(_player.allHeroes[i], i,  Timings.HeroRow);
            yield return null;
        }
    }
    
    public static bool CanAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit)
    {
        if (!attackingUnit || !attackedUnit) return false;

        var attackingUnitCoordinates = BoardManager.Instance.GetCoordinatesForUnitBehaviour(attackingUnit);
        var attackedUnitCoordinates = BoardManager.Instance.GetCoordinatesForUnitBehaviour(attackedUnit);

        if (attackingUnitCoordinates == null || attackedUnitCoordinates == null) return false;
        
        
        if ((attackingUnitCoordinates.Value.column == attackedUnitCoordinates.Value.column) && 
            Mathf.Abs(attackingUnitCoordinates.Value.row - attackedUnitCoordinates.Value.row) <= attackingUnit.attackRange)
        {
            return true;
        }

        return false;
    }
    
    public IEnumerator ExecuteAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit)
    {
        yield return StartCoroutine(attackingUnit.Attack(attackedUnit));
    }
}
