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

    private void Start()
    {
        _player = Player.Instance;
    }
    
    private IEnumerator CreateHeroes()
    {
        foreach (var hero in _player.allHeroes)
        {
            boardManager.SpawnUnit(hero, Timings.HeroRow);
            yield return null;
        }
    }

    private IEnumerator TryEnemyAttacks()
    {
        var frontRowEnemies = boardManager.FrontRowEnemies;
        foreach (var enemy in frontRowEnemies)
        {
            if (!enemy.ShouldAttack()) continue;
            
            var enemyCoordinates = boardManager.GetCoordinatesForUnitBehaviour(enemy);
            if (enemyCoordinates != null)
            {
                var column = enemyCoordinates.Value.column;
                var hero = boardManager.GetUnitBehaviour(new Coordinates(column, Timings.HeroRow));
                if (hero) yield return StartCoroutine(enemy.Attack(hero));
            }
        }
    }
    
    public static bool CanAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit)
    {
        if (!attackingUnit || !attackedUnit) return false;

        var attackingUnitCoordinates = BoardManager.Instance.GetCoordinatesForUnitBehaviour(attackingUnit);
        var attackedUnitCoordinates = BoardManager.Instance.GetCoordinatesForUnitBehaviour(attackedUnit);

        if (attackingUnitCoordinates == null || attackedUnitCoordinates == null) return false;
        
        
        if ((attackingUnitCoordinates.Value.column == attackedUnitCoordinates.Value.column) && 
            Mathf.Abs(attackingUnitCoordinates.Value.row - attackedUnitCoordinates.Value.row) <= attackingUnit.unitData.attackRange)
        {
            return true;
        }

        return false;
    }
    
    public IEnumerator ExecuteAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit)
    {
        yield return StartCoroutine(attackingUnit.Attack(attackedUnit));
    }
    
    public IEnumerator SetEnemyAttackOrder(List<EnemyUnitBehaviour> enemiesInCombat)
    {
        // Remove dead units
        enemiesInCombat.RemoveAll(unit => unit == null || unit.isDead || unit.unitData.passive);

        // Order the list of units by their 'turnsTilAttack' property
        var orderedEnemies = enemiesInCombat.OrderBy(unit => unit.turnsTilAttack).ToList();

        foreach (var enemyUnitBehaviour in orderedEnemies)
        {
            enemyUnitBehaviour.EnableCountdownTimer();
            if (!enemyUnitBehaviour.healthUI.activeSelf) enemyUnitBehaviour.ShowAndUpdateHealth();
            if (!enemyUnitBehaviour.attackUI.activeSelf) enemyUnitBehaviour.ShowAttack();
        }

        orderedEnemies = orderedEnemies;
    
        yield break;
    }
}
