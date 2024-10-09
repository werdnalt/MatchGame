using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Wave", menuName = "ScriptableObjects/Wave", order = 1)]
public class Wave : ScriptableObject
{
    [FormerlySerializedAs("attacksUntilSpawn")] public int actionsUntilSpawn;
    public List<WaveEntry> units;
    public float timeBeforeSpawn;
    
    [Tooltip("Number of units to spawn during wave")]
    public int waveSize;
}
