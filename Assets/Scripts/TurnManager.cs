using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [SerializeField] private GameObject turnIndicatorPrefab;
    [SerializeField] private TextMeshProUGUI waveCounter;
    [SerializeField] private TextMeshProUGUI swapCounter;

    public List<UnitBehaviour> orderedEnemies;

    private void Awake()
    {
        _turnTaken = false;
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
        }
    }


}
