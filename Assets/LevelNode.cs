using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelNode : MonoBehaviour
{
    public bool isActive;

    [SerializeField] private List<Wave> waves;
}
