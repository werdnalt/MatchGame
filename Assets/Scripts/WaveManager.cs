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

    private int _numSwaps;

    public bool shouldSpawnWave
    {
        get
        {
            _numSwaps++;
            if (_numSwaps >= swapsUntilWave)
            {
                _numSwaps = 0;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    
    private int swapsUntilWave
    {
        get
        {
            if (waves.Count <= 0)
            {
                return -1;
            }

            return _upcomingWave.swapsUntilSpawn;
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
        _numSwaps = 0;
    }

    public List<Unit> GetUnitsToSpawn()
    {
        if (_upcomingWave == null) Debug.LogAssertion("Should not attempt to spawn a wave when there are no waves remaining");
        
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
