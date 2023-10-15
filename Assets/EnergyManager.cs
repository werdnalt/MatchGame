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
            
    private Tween flashTween; // Store reference to the flashing tween so we can kill it in StopFlashing

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
            orb.GetComponent<Image>().material.SetFloat("_GreyscaleBlend", 1); 
        }

        for (var i = 0; i < _currentEnergy; i++)
        {
            actionOrbs[i].GetComponent<Image>().material.SetFloat("_GreyscaleBlend", 0); 
        }
    }

    public void GainEnergy(int amount)
    {
        _currentEnergy += amount; // add the energy
        _currentEnergy = Mathf.Clamp(_currentEnergy, 0, maxEnergy); // clamp the energy within range
        
        var orb = actionOrbs[_currentEnergy - 1];
        
        var newScale = new Vector3(orb.transform.localScale.x * 1.05f, orb.transform.localScale.y * 1.05f, 1);
        orb.transform.DOPunchScale(newScale, .3f, 1, 1);
        orb.GetComponent<Image>().material.SetFloat("_GreyscaleBlend", 0); 
        
        UIManager.Instance.UpdateEnergyText(_currentEnergy);
    }

    public void SpendEnergy(int amount)
    {
        StopFlashing();
        _currentEnergy -= amount; // subtract the energy
        _currentEnergy = Mathf.Clamp(_currentEnergy, 0, maxEnergy); // clamp the energy within range
        
        var orb = actionOrbs[_currentEnergy];
        
        var newScale = new Vector3(orb.transform.localScale.x * 1.05f, orb.transform.localScale.y * 1.05f, 1);
        orb.transform.DOPunchScale(newScale, .3f, 1, 1);
        orb.GetComponent<Image>().material.SetFloat("_GreyscaleBlend", 1);
        
        UIManager.Instance.UpdateEnergyText(_currentEnergy);
    }

    public void FlashEnergy(int amount)
    {
        // gaining energy
        
        // losing energy
    }

    public bool DoHaveEnoughEnergy(int amount)
    {
        return amount <= _currentEnergy;
    }
    
    public void FlashOrb()
    {
        if (_currentEnergy - 1 >= 0 && _currentEnergy < actionOrbs.Count) // Check to ensure valid index
        {
            Debug.Log("flash orb");
            var orb = actionOrbs[_currentEnergy - 1];
            var orbImage = orb.GetComponent<Image>();

            // Tween between 0 and 1 for greyscale
            flashTween = orbImage.material.DOFloat(1, "_GreyscaleBlend", 0.5f) // 0.5f is the duration of the tween; adjust as needed
                .SetLoops(-1, LoopType.Yoyo) // Infinite yoyo loop to tween back and forth
                .SetEase(Ease.Linear); // Linear ease for constant speed
        }
    }

    public void StopFlashing()
    {
        if (_currentEnergy - 1>= 0 && _currentEnergy < actionOrbs.Count) // Check to ensure valid index
        {
            var orb = actionOrbs[_currentEnergy - 1];
            var orbImage = orb.GetComponent<Image>();

            // Kill the tween if it's active
            if (flashTween != null && flashTween.IsActive())
            {
                flashTween.Kill();
            }

            // Reset orb to 0 greyscale
            orbImage.material.SetFloat("_GreyscaleBlend", 0); 
        }
    }
}
