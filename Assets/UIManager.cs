using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject unitPanel;
    [SerializeField] private GameObject effectTextParent;
    [SerializeField] private Image unitPortrait;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject effectTextPrefab;

    private List<GameObject> _instantiatedEffectPrefabs = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ShowUnitPanel(UnitBehaviour unitBehaviour)
    {
        for (var i = _instantiatedEffectPrefabs.Count - 1; i >= 0; i--)
        {
            Destroy(_instantiatedEffectPrefabs[i].gameObject);
        }
        
        unitPanel.SetActive(true);
        unitPortrait.sprite = unitBehaviour.unitData.unitSprite;
        attackText.text = unitBehaviour._attack.ToString();
        healthText.text = ($"{unitBehaviour.currentHp}");
        nameText.text = unitBehaviour.unitData.displayName;

        foreach (var effect in unitBehaviour.unitData.effects)
        {
            var effectTextInstance = Instantiate(effectTextPrefab, effectTextParent.transform);
            effectTextInstance.GetComponentInChildren<TextMeshProUGUI>().text = effect.effectDescription;
            
            _instantiatedEffectPrefabs.Add(effectTextInstance);
        }
    }

    public void HideUnitPanel()
    {
        unitPanel.SetActive(false);

        for (var i = _instantiatedEffectPrefabs.Count - 1; i >= 0; i--)
        {
            Destroy(_instantiatedEffectPrefabs[i].gameObject);
        }
    }
}
