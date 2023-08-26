using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [SerializeField] private GameObject turnIndicatorPrefab;
    [SerializeField] private TextMeshProUGUI swapCounter;

    public List<UnitBehaviour> orderedCombatUnits;
    private List<GameObject> _turnIndicators;

    public int numSwapsBeforeCombat;
    private int _currentNumSwaps;

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

    public void ChooseTurnOrder(List<UnitBehaviour> unitsInCombat)
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
    }

    public void RemoveUnit(UnitBehaviour unit)
    {
        unit.isDead = true;
        if (!orderedCombatUnits.Contains(unit)) return;
        
        foreach (var turnIndicator in _turnIndicators)
        {
            var u = turnIndicator.GetComponent<TurnIndicator>().unitBehaviour;
            if (turnIndicator == null || u != unit) continue;
            
            turnIndicator.SetActive(false);
        }
    }

    public void ProcessTurn()
    {
        _currentNumSwaps++;
        if (_currentNumSwaps >= numSwapsBeforeCombat)
        {
            BoardManager.Instance.PerformCombat();
            _currentNumSwaps = 0;
        }
        UpdateSwapCounter();
    }

    private void UpdateSwapCounter()
    {
        var numSwapsLeft = numSwapsBeforeCombat - _currentNumSwaps;

        if (numSwapsLeft == 1)
        {
            swapCounter.text = ($"final move");
        }
        else
        {
            swapCounter.text = ($"{numSwapsBeforeCombat - _currentNumSwaps} moves left");
        }
    }
    
    
}
