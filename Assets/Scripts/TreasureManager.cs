using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureManager : MonoBehaviour
{
    public static TreasureManager Instance;

    public GameObject treasureBehaviourPrefab;

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

    public void AddTreasure(Treasure treasure)
    {
        var newTreasure = Instantiate(treasureBehaviourPrefab, transform);
        newTreasure.GetComponent<Image>().sprite = treasure.treasureSprite;
        newTreasure.GetComponent<TreasureBehaviour>().treasure = treasure;
    }
}
