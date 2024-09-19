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
    
    public IEnumerator CreateHeroes()
    {
        foreach (var hero in _player.allHeroes)
        {
            boardManager.SpawnUnit(hero, Timings.HeroRow);
            yield return null;
        }
    }

    public IEnumerator TryEnemyAttacks()
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
            Mathf.Abs(attackingUnitCoordinates.Value.row - attackedUnitCoordinates.Value.row) <= attackingUnit._unitData.attackRange)
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
