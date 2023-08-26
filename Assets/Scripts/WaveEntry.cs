using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveEntry", menuName = "ScriptableObjects/WaveEntry", order = 2)]
public class WaveEntry : ScriptableObject
{
    public Unit unit;
    public int weight;
}
