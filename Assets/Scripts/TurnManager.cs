using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [SerializeField] private GameObject turnIndicatorPrefab;

    private List<GameObject> _turnIndicators;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
        }

        _turnIndicators = new List<GameObject>();
    }

    public void ChooseTurnOrder(List<UnitBehaviour> unitsInCombat)
    {
        // Use a dictionary to store the unit and its corresponding roll
        Dictionary<UnitBehaviour, int> unitRolls = new Dictionary<UnitBehaviour, int>();

        // iterate through every unit in unitsInCombat
        foreach (var unit in unitsInCombat)
        {
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
            var turnIndicatorInstance = Instantiate(turnIndicatorPrefab, transform);
            
            var turnIndicator = turnIndicatorInstance.GetComponent<TurnIndicator>();
            turnIndicator.unitSprite.sprite = unit.unit.unitSprite;
            turnIndicator.unitBehaviour = unit;
            _turnIndicators.Add(turnIndicatorInstance);
        }
    }

    private void ProcessTurn()
    {
        
    }
    
    
}
