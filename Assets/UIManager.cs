using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [SerializeField] private GameObject chestOverlay;
    [SerializeField] private Image treasureImage;
    [SerializeField] private TextMeshProUGUI treasureNameText;
    [SerializeField] private TextMeshProUGUI treasureEffectText;

    [SerializeField] private GameObject unitPanel;
    [SerializeField] private GameObject effectTextParent;
    [SerializeField] private Image unitPortrait;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI tribeText;
    [SerializeField] private Image tribePlacard;
    [SerializeField] private GameObject effectTextPrefab;

    [SerializeField] private TextMeshProUGUI energyText;

    private List<GameObject> _instantiatedEffectPrefabs = new List<GameObject>();

    private Vector3 _originalPos;
    private PopEffect _popEffect;

    public bool chestDestroyed;
    private List<Treasure> _currentTreasure;
    private Treasure _chosenTreasure;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _originalPos = transform.localPosition;
        _popEffect = unitPanel.GetComponent<PopEffect>();
    }

    public void ShowUnitPanel(UnitBehaviour unitBehaviour)
    {
        for (var i = _instantiatedEffectPrefabs.Count - 1; i >= 0; i--)
        {
            Destroy(_instantiatedEffectPrefabs[i].gameObject);
        }
        
        _popEffect.EnableAndPop();

        var panelPos = new Vector3(unitBehaviour.transform.position.x + 3.5f, unitBehaviour.transform.position.y, 1);
        unitPanel.transform.position = panelPos;
        
        var endingPos = new Vector3(0, 30f, 0);

        switch (unitBehaviour.unitData.tribe)
        {
            case Unit.Tribe.Beasts:
                tribePlacard.color = new Color32(249, 194, 43, 255);
                break;
            case Unit.Tribe.Void:
                tribePlacard.color = new Color32(107,62,117, 255);
                break;
            case Unit.Tribe.Plants:
                tribePlacard.color = new Color32(213,224,75, 255);
                break;
        }
        
        attackText.text = unitBehaviour.attack.ToString();

        healthText.text = ($"{unitBehaviour.currentHp}");
        nameText.text = unitBehaviour.unitData.displayName;
        tribeText.text = unitBehaviour.unitData.tribe.ToString();
        
        foreach (var effect in unitBehaviour.unitData.effects)
        {
            var effectTextInstance = Instantiate(effectTextPrefab, effectTextParent.transform);
            Debug.Log("spawned effect prefab");
            // var effectBehaviour = effectTextInstance.GetComponent<EffectBehaviour>();
            //
            // if (!effectBehaviour) continue;
            
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

    public void UpdateEnergyText(int amountLeft)
    {
        energyText.text = $"Actions: {amountLeft}";
    }

    public void ChestDestroyed(List<Treasure> treasures)
    {
        chestDestroyed = true;
        _currentTreasure = treasures;
    }

    public IEnumerator ChestEvent()
    {
        SetUpChestEvent();
        
        yield return new WaitUntil(() => !chestDestroyed);
    }

    private void SetUpChestEvent()
    {
        var randomTreasure = _currentTreasure[Random.Range(0, _currentTreasure.Count)];
        _chosenTreasure = randomTreasure;
        
        chestOverlay.GetComponent<PopEffect>().EnableAndPop();
        treasureImage.sprite = randomTreasure.treasureSprite;
        treasureNameText.text = randomTreasure.name;
        treasureEffectText.text = randomTreasure.treasureDescription;
    }

    public void CollectTreasure()
    {
        TreasureManager.Instance.AddTreasure(_chosenTreasure);
        chestOverlay.SetActive(false);
        chestDestroyed = false;
    }
}
