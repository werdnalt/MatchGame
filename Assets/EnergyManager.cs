using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    public static EnergyManager Instance;
    
    private int _currentEnergy;
    public int startingEnergy;
    public int maxEnergy;

    public int energyPerSwap = 1;
    public int energyGainedPerAttack = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _currentEnergy = startingEnergy;
    }

    private void Start()
    {
        UIManager.Instance.UpdateEnergyText(_currentEnergy);
    }

    public void GainEnergy(int amount)
    {
        _currentEnergy = Mathf.Clamp(_currentEnergy += amount, 0, maxEnergy);
        UIManager.Instance.UpdateEnergyText(_currentEnergy);
    }

    public void SpendEnergy(int amount)
    {
        _currentEnergy = Mathf.Clamp(_currentEnergy -= amount, 0, maxEnergy);
        UIManager.Instance.UpdateEnergyText(_currentEnergy);
    }

    public bool DoHaveEnoughEnergy(int amount)
    {
        return amount <= _currentEnergy;
    }
}
