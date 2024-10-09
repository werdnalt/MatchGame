using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    public List<Wave> waves;

    private int _numActions;

    public int totalNumWaves;
    public int currentNumWave;

    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI actionsUntilWaveText;

    public bool shouldSpawnWave
    {
        get
        {
            Debug.Log($"Num Actions: {_numActions}, Actions Until Wave: {ActionsUntilWave}");
            if (_numActions >= ActionsUntilWave)
            {
                _numActions = 0;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public int attacksLeftUntilWave
    {
        get
        {
            return ActionsUntilWave - _numActions;
        }
    }
    
    public int ActionsUntilWave
    {
        get
        {
            if (waves.Count <= 0)
            {
                return -1;
            }

            return _upcomingWave.actionsUntilSpawn;
        }
    }

    private Wave _upcomingWave {
        get
        {
            if (waves.Count <= 0)
            {
                return null;
            }
            return waves.First();
        }
    }
    
    private int _wavesCompleted = 0;
    private List<Unit> _unitsToSpawn;

    [SerializeField] private TextMeshProUGUI timeUntilWaveText;
    [SerializeField] private TextMeshProUGUI waveNumberText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        _numActions = 0;

        currentNumWave = 0;
        totalNumWaves = waves.Count;

        EventPipe.OnActionTaken += UpdateActions;
    }
    
    public List<Unit> GetUnitsToSpawn()
    {
        if (waves.Count == 0) return null;
        if (_upcomingWave == null) Debug.LogAssertion("Should not attempt to spawn a wave when there are no waves remaining");

        currentNumWave++;
        waveText.text = $"WAVE {currentNumWave} / {waves.Count}";
        
        var toSpawn = new List<Unit>();
        for (var i = 0; i < _upcomingWave.waveSize; i++)
        {
            var selectedUnit = SelectWeightedRandomUnit();
            if (selectedUnit != null)
            {
                toSpawn.Add(selectedUnit);
            }
        }

        waves.RemoveAt(0);
        actionsUntilWaveText.text = $"{ActionsUntilWave - _numActions}";
        return toSpawn;
    }
    
    private void UpdateActions(int actions)
    {
        _numActions += actions;
        actionsUntilWaveText.text = $"{ActionsUntilWave - _numActions}";
    }
    
    private Unit SelectWeightedRandomUnit()
    {
        if (_upcomingWave.units.Count == 0) return null;

        var totalWeight = 0;
        foreach (var unit in _upcomingWave.units)
        {
            totalWeight += unit.weight;
        }

        var randomValue = Random.Range(0, totalWeight);
        foreach (var entry in _upcomingWave.units)
        {
            if (randomValue < entry.weight)
            {
                return entry.unit;
            }
            randomValue -= entry.weight;
        }

        return null;
    }
}
