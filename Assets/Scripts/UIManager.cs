using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public InputActionAsset inputAction;
    
    [SerializeField] public GameObject chestOverlay;
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
    [SerializeField] private List<TreasureChoiceBehaviour> treasureChoices;
    [SerializeField] private TextMeshProUGUI treasureReceivedText;
    [SerializeField] private RectTransform tooltipRectTransform;
    
    [SerializeField] private TextMeshProUGUI energyText;
    
    [SerializeField] private GameObject toolTip;
    private TextMeshProUGUI _toolTipText;

    private List<GameObject> _instantiatedEffectPrefabs = new List<GameObject>();
    

    private Vector3 _originalPos;
    private PopEffect _popEffect;
    
    private Vector2 _mousePos;
    private InputAction _mousePositionAction;

    public GameObject heroUIParent;
    public bool chestDestroyed;
    private List<Treasure> _currentTreasure;
    public Treasure chosenTreasure;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        _mousePositionAction = inputAction.FindAction("MousePosition"); // Use the name of your action here
        if (_mousePositionAction != null)
        {
            _mousePositionAction.Enable();
        }
    }

    private void Update()
    {
        if (_mousePositionAction != null)
        {
            _mousePos = _mousePositionAction.ReadValue<Vector2>();
        }
    
        if (toolTip.activeSelf)
        {
            GetMouseWorldPosWithOffset();
        }
    }

    private void GetMouseWorldPosWithOffset()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -Camera.main.transform.position.z));

        // Adjust the offset value here
        float offsetInPixels = 100f;  // Change this value as needed

        float yOffset = offsetInPixels / Camera.main.pixelHeight * (Camera.main.orthographicSize * 2);

        // Check if the cursor is in the top or bottom half of the screen
        if (mouseScreenPos.y > Camera.main.pixelHeight / 2)
        {
            // Cursor is in the top half, so offset below
            worldPos.y -= yOffset;
            tooltipRectTransform.pivot = new Vector2(.5f, 1);
        }
        else
        {
            offsetInPixels = 150f;
            yOffset = offsetInPixels / Camera.main.pixelHeight * (Camera.main.orthographicSize * 2);
            // Cursor is in the bottom half, so offset above
            worldPos.y += yOffset;
            tooltipRectTransform.pivot = new Vector2(.5f, 0);
        }
        
        toolTip.transform.position = worldPos;
    }

    private void Start()
    {
        _originalPos = transform.localPosition;
        _popEffect = unitPanel.GetComponent<PopEffect>();

        _toolTipText = toolTip.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ShowUnitPanel(UnitBehaviour unitBehaviour)
    {
        for (var i = _instantiatedEffectPrefabs.Count - 1; i >= 0; i--)
        {
            Destroy(_instantiatedEffectPrefabs[i].gameObject);
        }
        
        _popEffect.EnableAndPop();

        // var panelPos = new Vector3(unitBehaviour.transform.position.x + 3.5f, unitBehaviour.transform.position.y, 1);
        // unitPanel.transform.position = panelPos;
        
        var endingPos = new Vector3(0, 30f, 0);

        switch (unitBehaviour._unitData.tribe)
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
        nameText.text = unitBehaviour._unitData.displayName;
        tribeText.text = unitBehaviour._unitData.tribe.ToString();
        
        foreach (var effectState in unitBehaviour.effects)
        {
            var effectTextInstance = Instantiate(effectTextPrefab, effectTextParent.transform);
            Debug.Log("spawned effect prefab");
            // var effectBehaviour = effectTextInstance.GetComponent<EffectBehaviour>();
            //
            // if (!effectBehaviour) continue;
            
            effectTextInstance.GetComponentInChildren<TextMeshProUGUI>().text = effectState.effect.effectDescription;
            if (effectState.isSilenced)
            {
                effectTextInstance.GetComponentInChildren<TextMeshProUGUI>().text =
                    ($"<s>{effectState.effect.effectDescription}</s>");
            }
            
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
        treasureReceivedText.text = "CHOOSE A TREASURE!";
        int maxAttempts = 25;  // Define a max number of attempts to get a different treasure.

        for (var i = 0; i < treasureChoices.Count; i++)
        {
            treasureChoices[i].gameObject.SetActive(true);
            HashSet<Treasure> obtainedTreasures = new HashSet<Treasure>();
    
            Treasure randomTreasure = null;
            int attempt = 0;
            do
            {
                randomTreasure = _currentTreasure[Random.Range(0, _currentTreasure.Count)];
                attempt++;
        
                if (attempt > maxAttempts)
                {
                    // If we've tried too many times, just use the last treasure we picked.
                    break;
                }
            }
            while (obtainedTreasures.Contains(randomTreasure));
    
            obtainedTreasures.Add(randomTreasure);
            treasureChoices[i].SetTreasure(randomTreasure);
        }
        
        chestOverlay.GetComponent<PopEffect>().EnableAndPop();
    }

    public void ShowToolTip(string textToDisplay)
    {
        toolTip.GetComponent<PopEffect>().EnableAndPop();
        _toolTipText.text = textToDisplay;
    }

    public void HideToolTip()
    {
        toolTip.SetActive(false);
    }
    
    public void HideTreasurePopup()
    {
        chestOverlay.GetComponent<PopEffect>().DisableAndPop();
        chestDestroyed = false;
    }
}
