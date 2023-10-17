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

    private int _numAttacks;

    public int totalNumWaves;
    public int currentNumWave;

    public bool shouldSpawnWave
    {
        get
        {
            if (_numAttacks >= AttacksUntilWave)
            {
                _numAttacks = 0;
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
            return AttacksUntilWave - _numAttacks;
        }
    }
    
    public int AttacksUntilWave
    {
        get
        {
            if (waves.Count <= 0)
            {
                return -1;
            }

            return _upcomingWave.attacksUntilSpawn;
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
        _numAttacks = 0;

        currentNumWave = 0;
        totalNumWaves = waves.Count;
    }

    private void Start()
    {
        //waves = new List<Wave>(GameManager.Instance.levelData.waves);
    }

    public void IncrementAttackCounter()
    {
        _numAttacks++;
    }

    public List<Unit> GetUnitsToSpawn()
    {
        if (waves.Count == 0) waves = new List<Wave>(GameManager.Instance.levelData.waves);
        if (_upcomingWave == null) Debug.LogAssertion("Should not attempt to spawn a wave when there are no waves remaining");

        currentNumWave++;
        
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
        return toSpawn;
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
