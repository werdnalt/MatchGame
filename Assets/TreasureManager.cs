using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureManager : MonoBehaviour
{
    public static TreasureManager Instance;

    public TreasureBehaviour heldTreasure;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PickupTreasure(TreasureBehaviour treasureBehaviour)
    {
        heldTreasure = treasureBehaviour;
    }

    public void DropTreasure()
    {
        heldTreasure = null;
    }
}
