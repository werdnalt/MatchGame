using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public IEnumerator LevelUpUnits()
    {
        var doneLeveling = false;

        yield return new WaitUntil(() => doneLeveling);
    }
}
