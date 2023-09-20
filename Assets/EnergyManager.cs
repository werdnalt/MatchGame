using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnergyManager : MonoBehaviour
{
    public static EnergyManager Instance;
    
    private int _currentEnergy;
    public int startingEnergy;
    public int maxEnergy;

    public int energyPerSwap = 1;
    public int energyGainedPerAttack = 1;
    
    public List<GameObject> actionOrbs;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _currentEnergy = startingEnergy;
    }

    private void Start()
    {
        UIManager.Instance.UpdateEnergyText(_currentEnergy);

        foreach (var orb in actionOrbs)
        {
            orb.GetComponent<Image>().material = new Material(orb.GetComponent<Image>().material);
        }
    }

    public void GainEnergy(int amount)
    {
        _currentEnergy += amount; // add the energy
        _currentEnergy = Mathf.Clamp(_currentEnergy, 0, maxEnergy); // clamp the energy within range

        // Re-enable the amount of UI elements according to gained energy
        for (var i = _currentEnergy - 1; i >= _currentEnergy - amount && i >= 0; i--)
        {
            Vector3 newScale = new Vector3(actionOrbs[i].transform.localScale.x * 1.05f, actionOrbs[i].transform.localScale.y * 1.05f, 1);
            actionOrbs[i].transform.DOPunchScale(newScale, .3f, 1, 1);
            actionOrbs[i].GetComponent<Image>().material.SetFloat("_GreyscaleBlend", 0); 
        }

        UIManager.Instance.UpdateEnergyText(_currentEnergy);
    }

    public void SpendEnergy(int amount)
    {
        // Disable the amount of UI elements according to spent energy
        for (var i = _currentEnergy - 1; i >= _currentEnergy - amount && i >= 0; i--)
        {        
            Vector3 newScale = new Vector3(actionOrbs[i].transform.localScale.x * 1.05f, actionOrbs[i].transform.localScale.y * 1.05f, 1);
            actionOrbs[i].transform.DOPunchScale(newScale, .3f, 1, 1);
            actionOrbs[i].GetComponent<Image>().material.SetFloat("_GreyscaleBlend", 1);
        }
    
        _currentEnergy -= amount; // subtract the energy
        _currentEnergy = Mathf.Clamp(_currentEnergy, 0, maxEnergy); // clamp the energy within range

        UIManager.Instance.UpdateEnergyText(_currentEnergy);
    }

    public bool DoHaveEnoughEnergy(int amount)
    {
        return amount <= _currentEnergy;
    }
}
