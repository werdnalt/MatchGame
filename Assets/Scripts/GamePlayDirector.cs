using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayDirector : MonoBehaviour
{
    public static GamePlayDirector Instance;
    public bool PlayerActionAllowed;

    private BoardManager _boardManager;
    private UnitManager _unitManager;

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

        PlayerActionAllowed = false;
    }

    private void Start()
    {
        _boardManager = FindObjectOfType<BoardManager>();
        _unitManager = FindObjectOfType<UnitManager>();

        EventPipe.OnActionTaken += OnPlayerAction;
        EventPipe.OnPlayerAttack += HandlePlayerAttack;

        StartCoroutine(StartLevel());
    }

    private void HandlePlayerAttack()
    {
        PlayerActionAllowed = false;
    }

    private IEnumerator StartLevel()
    {
        // Create board
        yield return StartCoroutine(_boardManager.CreateBoard());
        
        // Spawn Heroes and Enemies
        yield return StartCoroutine(_unitManager.CreateHeroes());

        if (WaveManager.Instance.shouldSpawnWave)
        {
            var waveToSpawn = WaveManager.Instance.GetUnitsToSpawn();
            yield return StartCoroutine(_boardManager.SpawnEnemyUnits(waveToSpawn));
        }
        
        PlayerActionAllowed = true;
    }

    private void OnPlayerAction(int actionCost)
    {
        PlayerActionAllowed = false;
        StartCoroutine(HandlePlayerAction(actionCost));
    }

    private IEnumerator HandlePlayerAction(int actions)
    {
        yield return new WaitUntil(() => !UIManager.Instance.chestDestroyed);
        
        StartCoroutine(_boardManager.CleanUpBoard());
        // check for any enemies that should attack
        yield return StartCoroutine(_unitManager.HandlePlayerActionTaken(actions));
        
        if (WaveManager.Instance.shouldSpawnWave)
        {
            var waveToSpawn = WaveManager.Instance.GetUnitsToSpawn();
            yield return StartCoroutine(_boardManager.SpawnEnemyUnits(waveToSpawn));
        }

        PlayerActionAllowed = true;
    }
}