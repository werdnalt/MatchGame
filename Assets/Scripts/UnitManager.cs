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
    
    public IEnumerator AssignCombatTargets()
    {
        for (var i = 0; i < boardManager.numColumns; i++)
        {
            var hero = boardManager.Heroes[i];
            var enemy = boardManager.FrontRowEnemies[i];

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
    
    public static bool CanAttack(UnitBehaviour attackingUnit, UnitBehaviour attackedUnit)
    {
        if (!attackingUnit || !attackedUnit) return false;
        
        var combatTarget = attackingUnit.combatTarget;

        if (Mathf.Abs(attackedUnit.coordinates.x - combatTarget.coordinates.x) <= attackingUnit.unitData.attackRange && 
            Mathf.Abs(attackedUnit.coordinates.y - combatTarget.coordinates.y) <= attackingUnit.unitData.attackRange)
        {
            return true;
        }

        return false;
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
