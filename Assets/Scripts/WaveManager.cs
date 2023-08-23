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

    [SerializeField] private TextMeshProUGUI timeUntilWaveText;
    [SerializeField] private TextMeshProUGUI waveNumberText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // private void UpdateWaveUI()
    // {
    //     if (_upcomingWave != null)
    //     {
    //         var timeOfSpawn = _waveSpawnedTime + _upcomingWave.timeBeforeSpawn;
    //         var totalSeconds = (int)(timeOfSpawn - _ongoingTime);
    //         var minutes = totalSeconds / 60;
    //         var seconds = totalSeconds % 60;
    //
    //         timeUntilWaveText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
    //
    //         waveNumberText.text = $"Wave {_wavesCompleted} in";
    //     }
    //     else
    //     {
    //         waveNumberText.text = "Final Wave";
    //     }
    //    
    // }

    public Unit GetRandomUnitFromWave()
    {
        if (_upcomingWave == null) Debug.LogAssertion("Unable to get unit from wave; no waves remain");
        return _upcomingWave.units[Random.Range(0, _upcomingWave.units.Count)];
    }

    public void StartWaveSpawn()
    {
        if (_upcomingWave == null) Debug.LogAssertion("Should not attempt to spawn a wave when there are no waves remaining");
        BoardManager.Instance.SpawnWave(_upcomingWave);
        _wavesCompleted++; // Is this what we want to do?
        waves.RemoveAt(0);
    }
}
