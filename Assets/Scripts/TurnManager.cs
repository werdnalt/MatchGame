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
    [SerializeField] private TextMeshProUGUI swapCounter;

    public List<UnitBehaviour> orderedCombatUnits;
    private List<GameObject> _turnIndicators;

    public int numSwapsBeforeCombat;

    public int currentNumSwaps { get; private set; }

    private void Awake()
    {
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
        var combatOrder = 1;
        orderedCombatUnits.Clear();
        if (_turnIndicators.Count != 0)
        {
            for (var i = _turnIndicators.Count - 1; i >= 0; i--)
            {
                Destroy(_turnIndicators[i]);
            }
            _turnIndicators.Clear();
        }
        
        // Use a dictionary to store the unit and its corresponding roll
        Dictionary<UnitBehaviour, int> unitRolls = new Dictionary<UnitBehaviour, int>();

        // iterate through every unit in unitsInCombat
        foreach (var unit in unitsInCombat)
        {
            if (unit == null || unit.unit.passive) continue;

            // roll a random number from 1-100
            int roll = Random.Range(1, 101);
            
            // Ensure that we don't have a duplicate roll (This is optional and handles the edge case where two units get the same roll)
            while (unitRolls.Values.Contains(roll))
            {
                roll = Random.Range(1, 101);
            }

            unitRolls.Add(unit, roll);
        }

        // Order the dictionary based on the roll values and then convert to a list of units
        var orderedUnits = unitRolls.OrderBy(pair => pair.Value).Select(pair => pair.Key).ToList();

        foreach (var unit in orderedUnits)
        {
            orderedCombatUnits.Add(unit);
            unit.combatOrderText.text = combatOrder.ToString();
            combatOrder++;
            
            var turnIndicatorInstance = Instantiate(turnIndicatorPrefab, transform);
            
            var turnIndicator = turnIndicatorInstance.GetComponent<TurnIndicator>();
            turnIndicator.unitSprite.sprite = unit.unit.unitSprite;
            turnIndicator.unitBehaviour = unit;
            _turnIndicators.Add(turnIndicatorInstance);
        }
        yield break;
    }

    public void RemoveUnit(UnitBehaviour unit)
    {
        if (!orderedCombatUnits.Contains(unit)) return;
        
        foreach (var turnIndicator in _turnIndicators)
        {
            var u = turnIndicator.GetComponent<TurnIndicator>().unitBehaviour;
            if (turnIndicator == null || u != unit) continue;
            
            turnIndicator.SetActive(false);
        }
    }

    public void SwapBlocks()
    {
        currentNumSwaps++;
        UpdateSwapCounter();
    }

    public IEnumerator CheckIfFinishedSwapping()
    {
        UpdateSwapCounter();
        while (currentNumSwaps < numSwapsBeforeCombat)
        {
            yield return null;
        }

        BoardManager.Instance.canMove = false;
        currentNumSwaps = 0;
    }

    private void UpdateSwapCounter()
    {
        var numSwapsLeft = numSwapsBeforeCombat - currentNumSwaps;

        if (numSwapsLeft == 1)
        {
            swapCounter.text = ($"final move");
        }

        if (numSwapsLeft == 0)
        {
            swapCounter.text = ($"combat in progress");
        }
        else
        {
            swapCounter.text = ($"{numSwapsBeforeCombat - currentNumSwaps} moves left");
        }
    }
    
    
}
