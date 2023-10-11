using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [SerializeField] private GameObject turnIndicatorPrefab;
    [SerializeField] private TextMeshProUGUI waveCounter;
    [SerializeField] private TextMeshProUGUI swapCounter;

    public List<UnitBehaviour> orderedCombatUnits;
    private List<GameObject> _turnIndicators;
    private bool _hasSwapped;
    
    public int numSwapsBeforeCombat;

    public int currentNumSwaps { get; private set; }

    private void Awake()
    {
        _hasSwapped = false;
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
        }

        _turnIndicators = new List<GameObject>();
        UpdateSwapCounter();
    }

    public IEnumerator ChooseTurnOrder(List<UnitBehaviour> unitsInCombat)
    {
        orderedCombatUnits.Clear();

        // Clear previous turn indicators
        if (_turnIndicators.Count != 0)
        {
            for (var i = _turnIndicators.Count - 1; i >= 0; i--)
            {
                Destroy(_turnIndicators[i]);
            }
            _turnIndicators.Clear();
        }

        // Remove dead units
        unitsInCombat.RemoveAll(unit => unit == null || unit.isDead || unit.unitData.passive);

        // Order the list of units by their 'turnsTilAttack' property
        var orderedUnits = unitsInCombat.OrderBy(unit => unit.turnsTilAttack).ToList();

        foreach (var unit in orderedUnits)
        {
            unit.EnableCountdownTimer();
            if (!unit.healthUI.activeSelf) unit.ShowAndUpdateHealth();
            if (!unit.attackUI.activeSelf) unit.ShowAttack();
        }

        orderedCombatUnits = orderedUnits;
    
        yield break;
    }

    // public void RemoveUnit(UnitBehaviour unit)
    // {
    //     foreach (var turnIndicator in _turnIndicators)
    //     {
    //         var u = turnIndicator.GetComponent<TurnIndicator>().unitBehaviour;
    //         if (turnIndicator == null || u != unit) continue;
    //         
    //         turnIndicator.SetActive(false);
    //     }
    // }

    public IEnumerator TakeTurn()
    {
        yield return new WaitForSeconds(.25f);
        _hasSwapped = true;
        currentNumSwaps++;
        UpdateSwapCounter();
    }

    public IEnumerator CheckIfFinishedSwapping()
    {
        UpdateSwapCounter();
        while (!_hasSwapped)
        {
            yield return null;
        }

        BoardManager.Instance.CleanUpBoard();
        _hasSwapped = false;
    }

    public void UpdateSwapCounter()
    {
        var numSwapsLeft = WaveManager.Instance.swapsLeftUntilWave;
        waveCounter.text = $"Wave <color=#52bc9c>{WaveManager.Instance.currentNumWave} / {WaveManager.Instance.totalNumWaves}</color>";

        if (numSwapsLeft == 1)
        {
            swapCounter.text = ($"final move");
        }

        if (numSwapsLeft <= 0)
        {
            swapCounter.text = ($"Final Wave");
        }
        else
        {
            swapCounter.text = ($"Next Wave in <color=#52bc9c>{numSwapsLeft}</color> swaps");

        }
    }

    public void CountDownAttackTimers()
    {
        foreach (var unit in orderedCombatUnits)
        {
            StartCoroutine(unit.CountDownTimer());
        }
    }

    public void ResetUnit(UnitBehaviour unitBehaviour)
    {
        StartCoroutine(unitBehaviour.ResetAttackTimer());
    }
    
    public void ReinsertUnit(UnitBehaviour survivedUnit)
    {
        // If the unit is null or dead, do nothing
        if (survivedUnit == null || survivedUnit.isDead) return;
        
        // Remove the unit first to avoid duplicate
        orderedCombatUnits.Remove(survivedUnit);

        // Rebuild the UI
        RebuildCombatOrderUI();
    }

    private void RebuildCombatOrderUI()
    {
        // Remove all current turn indicators
        for (var i = _turnIndicators.Count - 1; i >= 0; i--)
        {
            Destroy(_turnIndicators[i]);
        }
        _turnIndicators.Clear();
        
        foreach (var unit in orderedCombatUnits)
        {
            var turnIndicatorInstance = Instantiate(turnIndicatorPrefab, transform);
            var turnIndicator = turnIndicatorInstance.GetComponent<TurnIndicator>();
            turnIndicator.unitSprite.sprite = unit.unitData.unitSprite;
            turnIndicator.unitBehaviour = unit;

            if (unit.unitData.tribe == Unit.Tribe.Hero)
            {
                turnIndicator.SetBackgroundColor(new Color32(120, 185, 65, 255));
            }

            _turnIndicators.Add(turnIndicatorInstance);
        }
    }
    
    
}
