using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialUnitBehaviour : UnitBehaviour
{
    public int disappearAfterSeconds;

    private void Update()
    {
        if (Time.time - disappearAfterSeconds >= timeSpawned)
        {
            //BoardManager.Instance.ReplaceWithRandomBlock(coordinates);
        }
    }
}
