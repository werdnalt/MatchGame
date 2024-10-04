using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    [SerializeField] private TreasureUIManager treasureUIManager;

    [SerializeField] private GameObject unitPanel;
    [SerializeField] private Image unitPortrait;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI tribeText;
    [SerializeField] private Image tribePlacard;
    [SerializeField] private List<TreasureChoiceBehaviour> treasureChoices;
    [SerializeField] private TextMeshProUGUI treasureReceivedText;
    [SerializeField] private RectTransform tooltipRectTransform;
    
    [SerializeField] private PopupTooltip popupTooltip;
    

    
    [SerializeField] private TextMeshProUGUI energyText;
    
    [SerializeField] private GameObject toolTip;
    private TextMeshProUGUI _toolTipText;

    private List<GameObject> _instantiatedEffectPrefabs = new List<GameObject>();
    

    private Vector3 _originalPos;
    private PopEffect _popEffect;
    
    private Vector2 _mousePos;
    private InputAction _mousePositionAction;
    private UnitBehaviour _heroWhoOpenedChest;

    public GameObject heroUIParent;
    public bool chestDestroyed;
    private List<Treasure> _currentTreasure;

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
        popupTooltip.ShowUnitPanel(unitBehaviour);
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

    public void ChestDestroyed(List<Treasure> treasures, UnitBehaviour heroWhoOpenedChest)
    {
        chestDestroyed = true;
        _heroWhoOpenedChest = heroWhoOpenedChest;
        _currentTreasure = treasures;

        StartCoroutine(ChestEvent());
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
        
        treasureUIManager.EnableUI();
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
    
    public IEnumerator AwardTreasure(TreasureChoiceBehaviour chosenTreasure)
    {
        treasureUIManager.HideUI();
        yield return StartCoroutine(AnimateTreasureToHero(chosenTreasure));
        _heroWhoOpenedChest.AddEffect(chosenTreasure.treasure.effect);
        EventPipe.AddTreasure(new HeroAndTreasure(_heroWhoOpenedChest, chosenTreasure.treasure));
        chestOverlay.GetComponent<PopEffect>().DisableObject();
        chestDestroyed = false;
    }

    private IEnumerator AnimateTreasureToHero(TreasureChoiceBehaviour treasure)
    {
        var duration = .75f;
        // Get the hero's world position
        Vector3 worldPosition = _heroWhoOpenedChest.transform.position;
        Debug.Log($"Hero world pos: {worldPosition}");
    
        // Convert the hero's world position to screen space
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        Debug.Log($"Hero screen pos: {screenPosition}");

        // Get the starting position of the treasure in screen space
        Vector3 startPosition = treasure.transform.position;

        // Create a control point that is higher than the start and target positions (for an arc)
        Vector3 controlPoint = (startPosition + screenPosition) / 2f;  // Midpoint between start and target
        controlPoint.y += 1000f;  // Raise the control point slightly to create the arc (adjust the value for height)

        // Define the path (start, control point, and target)
        Vector3[] path = new Vector3[] { startPosition, controlPoint, screenPosition };

        // Animate the treasure scaling down
        treasure.transform.DOScale(new Vector3(.3f, .3f, .3f), 0.3f).SetEase(Ease.OutQuad);

        // Animate along the path using DOPath
        treasure.transform.DOPath(path, duration, PathType.CatmullRom).SetEase(Ease.OutQuad);

        // Wait for the animation to finish
        yield return new WaitForSeconds(duration);
    }

    

    
}
