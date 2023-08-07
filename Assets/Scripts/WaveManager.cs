using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;
    
    public List<Wave> waves;

    private float _startTime;
    private float _ongoingTime;
    private float _waveSpawnedTime;
    private bool _isTimeOngoing;
    private Wave _upcomingWave;
    private int _wavesCompleted = 0;

    [SerializeField] private TextMeshProUGUI timeUntilWaveText;
    [SerializeField] private TextMeshProUGUI waveNumberText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _upcomingWave = waves[_wavesCompleted];

        _isTimeOngoing = true;
        _ongoingTime = 0f;
        _waveSpawnedTime = Time.time;
    }

    public void Update()
    {
        if (_isTimeOngoing) _ongoingTime += Time.deltaTime;

        if (_ongoingTime >= _waveSpawnedTime + _upcomingWave.timeBeforeSpawn)
        {
            StartWaveSpawn();
        }
        
        UpdateWaveUI();
    }

    private void SetUpcomingWave()
    {
        _wavesCompleted++;
        if (_wavesCompleted >= waves.Count) return;

        _upcomingWave = waves[_wavesCompleted];
    }

    private void UpdateWaveUI()
    {
        var timeOfSpawn = _waveSpawnedTime + _upcomingWave.timeBeforeSpawn;
        var totalSeconds = (int)(timeOfSpawn - _ongoingTime);
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;

        timeUntilWaveText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);

        waveNumberText.text = $"Wave {_wavesCompleted} in";
    }

    public Unit GetRandomUnitFromWave()
    {
        var currentWave = waves[_wavesCompleted];
        return currentWave.units[Random.Range(0, currentWave.units.Count)];
    }

    private void StartWaveSpawn()
    {
        BoardManager.Instance.SpawnWave(_upcomingWave);
        SetUpcomingWave();
        _waveSpawnedTime = Time.time;
    }
}
