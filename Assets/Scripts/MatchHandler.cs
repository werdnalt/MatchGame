﻿using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;

public class MatchHandler : MonoBehaviour
{
    public static MatchHandler Instance;

    public GameObject MatchDisplayerGameObject;
    public GameObject MatchDisplayObjectLayout;
    public GameObject MatchDisplayObjectPrefab;
    public TextMeshProUGUI comboText;

    private List<GameObject> instantiatedDisplayObjectPrefabs = new List<GameObject>();
    private Queue<MatchEffect> _queuedMatchEffects = new Queue<MatchEffect>();

    public enum MatchEffect
    {
        Damage1,
        Damage2,
        Damage3,
        Damage4,
        Damage5,
        Heal1,
        Heal2,
        Heal3,
        Heal4,
        Heal5,
        Stun5,
        Stun10
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Sword(int runLength)
    {

    }

    private void EnemyEffect(int runLength)
    {
        // switch (runLength)
        // {
        //     case 3:
        //         Player.Instance.TakeDamage(2);
        //         break;
        //
        //     case 4:
        //         Player.Instance.TakeDamage(3);
        //         break;
        //
        //     case 5:
        //         Player.Instance.TakeDamage(4);
        //         break;
        // }
    }

    public void ShowMatchDisplayerUI()
    {
        MatchDisplayerGameObject.SetActive(true);
    }

    public void HideMatchDisplayerUI()
    {
        foreach (var prefab in instantiatedDisplayObjectPrefabs)
        {
            Destroy(prefab);
        }
        instantiatedDisplayObjectPrefabs.Clear();
        MatchDisplayerGameObject.SetActive(false);
    }

    public void AddMatchDisplayerObject(Sprite icon, string text)
    {
        GameObject matchDisplayer = Instantiate(MatchDisplayObjectPrefab, MatchDisplayObjectLayout.transform);
        matchDisplayer.transform.SetParent(MatchDisplayObjectLayout.transform);
        matchDisplayer.GetComponent<MatchDisplayer>().Setup(icon, text);
        instantiatedDisplayObjectPrefabs.Add(matchDisplayer);
    }

    private void QueueMatchEffect(MatchEffect matchEffect)
    {
        _queuedMatchEffects.Enqueue(matchEffect);
    }
}
