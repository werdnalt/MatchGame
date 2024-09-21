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

        StartCoroutine(StartLevel());
    }

    private IEnumerator StartLevel()
    {
        // Create board
        yield return StartCoroutine(_boardManager.CreateBoard());
        
        // Spawn Heroes and Enemies
        yield return StartCoroutine(_unitManager.CreateHeroes());

        var waveToSpawn = WaveManager.Instance.GetUnitsToSpawn();
        yield return StartCoroutine(_boardManager.SpawnEnemyUnits(waveToSpawn));

        PlayerActionAllowed = true;
    }

    private void OnPlayerAction()
    {
        StartCoroutine(HandlePlayerAction());
    }

    private IEnumerator HandlePlayerAction()
    {
        // check for any enemies that should attack
        yield return StartCoroutine(_unitManager.TryEnemyAttacks());
    }
}