using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CursorAnimation : MonoBehaviour, IPointerUpHandler
{
    public MyPlayerActions playerActions;
    public static CursorAnimation Instance;
    
    // The maximum and minimum scales for the pulse animation
    private Vector3 maxScale;
    private Vector3 minScale;

    private Vector3 _startingDragPos;
    
    private BoardManager.Coordinates _startingCell;
    private BoardManager.Coordinates _swappingCell;

    private int _startingIndex;
    private int _swappingIndex;
    
    private Vector2 _mousePos;
    private InputAction _mousePositionAction;
    public InputActionAsset inputAction;

    private bool _isSwappingLeft;

    [SerializeField] private GameObject cellReference; 
    
    [SerializeField] private Sprite smallSwappingSelector;
    [SerializeField] private Sprite regularSelector;
    [SerializeField] private Sprite swapSelector;

    private SpriteRenderer _spriteRenderer;

    public bool isDragging = false;

    public bool isDraggingHero = false;

    // Duration of each pulse (half of the full cycle since it will be one way only)
    private float pulseDuration = 0.5f;

    private Vector3 localScale;

    private List<UnitBehaviour> highlightedUnits = new List<UnitBehaviour>();

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _mousePositionAction = inputAction.FindAction("MousePosition");

        playerActions = new MyPlayerActions();
        playerActions.Player.Cancel.performed += ctx => CancelSelection();
    }

    private void Update()
    {
        if (!isDragging) return;

        if (isDraggingHero)
        {
            _mousePos = _mousePositionAction.ReadValue<Vector2>();

            if (_mousePos.x < _startingDragPos.x - 1 && _startingIndex > 0)
            {
                _swappingCell = new BoardManager.Coordinates(_startingCell.x - 1, _startingCell.y);
                _isSwappingLeft = true;
                _spriteRenderer.sprite = swapSelector;
            }
            else if (_mousePos.x > _startingDragPos.x + 1 && _startingCell.x + 1 < BoardManager.Instance.numColumns)
            {
                _swappingCell = new BoardManager.Coordinates(_startingCell.x + 1, _startingCell.y);
                _isSwappingLeft = false;
                _spriteRenderer.sprite = swapSelector;
            }
            else
            {
                _spriteRenderer.sprite = smallSwappingSelector;
                _swappingCell = new BoardManager.Coordinates(-1, -1);
            }
        
            SetSwapSelectorPosition();
        }

        else
        {
            _mousePos = _mousePositionAction.ReadValue<Vector2>();

            if (_mousePos.x < _startingDragPos.x - 1 && _startingCell.x > 0)
            {
                _swappingCell = new BoardManager.Coordinates(_startingCell.x - 1, _startingCell.y);
                _isSwappingLeft = true;
                _spriteRenderer.sprite = swapSelector;
            }
            else if (_mousePos.x > _startingDragPos.x + 1 && _startingCell.x + 1 < BoardManager.Instance.numColumns)
            {
                _swappingCell = new BoardManager.Coordinates(_startingCell.x + 1, _startingCell.y);
                _isSwappingLeft = false;
                _spriteRenderer.sprite = swapSelector;
            }
            else
            {
                _spriteRenderer.sprite = smallSwappingSelector;
                _swappingCell = new BoardManager.Coordinates(-1, -1);
            }
        
            SetSwapSelectorPosition();
        }
        
    }

    public void CancelSelection()
    {
        isDragging = false;
        SetSwapSelectorPosition();
        Debug.Log("Canceling");
        _spriteRenderer.sprite = smallSwappingSelector;
        _swappingCell = new BoardManager.Coordinates(-1, -1);
        StartPulsing();
    }
    
    private void SetSwapSelectorPosition()
    {
        if (_swappingCell.Equals(new BoardManager.Coordinates(-1, -1))) return;

        if (_isSwappingLeft)
        {
            transform.position = BoardManager.Instance.GetSelectorPosition(_swappingCell, _startingCell);
        }

        else
        {
            transform.position = BoardManager.Instance.GetSelectorPosition(_startingCell, _swappingCell);
        }
    }

    void Start()
    {
        localScale = transform.localScale;
        
        maxScale = new Vector3(localScale.x + .01f, localScale.x + .01f, localScale.z + .01f);
        minScale = new Vector3(localScale.x, localScale.y, localScale.z);
        StartPulsing();
    }

    public void StartPulsing()
    {
        transform.DOScale(maxScale, pulseDuration)
            .SetEase(Ease.InOutSine) // Smooth in and out
            .OnComplete(() =>
            {
                transform.DOScale(minScale, pulseDuration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(StartPulsing); // Recursively call to create a loop
            });
    }

    private void StopPulsing()
    {
        transform.DOKill();
    }

    public void ChangeColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }

    public void StartDraggingFrom(Vector3 position, BoardManager.Coordinates startingCell)
    {
        if (isDragging) return;
        
        StopPulsing();

        _spriteRenderer.sprite = smallSwappingSelector;
        _startingDragPos = position;
        isDragging = true;
        _startingCell = startingCell;
    }
    
    public void StartDraggingHeroFrom(Vector3 position, int startingIndex)
    {
        if (isDragging) return;

        _spriteRenderer.sprite = smallSwappingSelector;
        _startingDragPos = position;
        isDraggingHero = true;
        _startingIndex = startingIndex;
    }
    
    public void StopDraggingHero()
    {
        ChangeColor(Color.white);
        isDragging = false;
        _isSwappingLeft = false;

        if (!_swappingIndex.Equals(new BoardManager.Coordinates(0, 0)))
        {
            BoardManager.Instance.SetCellSelector(_swappingCell);
        }
        else
        {
            BoardManager.Instance.SetCellSelector(_startingCell);
        }
        
        if (!_swappingCell.Equals(new BoardManager.Coordinates(0, 0)))
        {
            BoardManager.Instance.SwapBlocks(_swappingCell, _startingCell);
        }
        
        _swappingCell = new BoardManager.Coordinates(0, 0);
        _spriteRenderer.sprite = regularSelector;
        
    }

    public void StopDragging()
    {
        StartPulsing();
        ChangeColor(Color.white);
        isDragging = false;
        _isSwappingLeft = false;
        
        BoardManager.Instance.SetCellSelector(_startingCell);
        BoardManager.Instance.SwapBlocks(_swappingCell, _startingCell);
        
        _spriteRenderer.sprite = regularSelector;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("pointer up");
        if (eventData.button == PointerEventData.InputButton.Right) CancelSelection();
    }

    public void HoldingAnimation()
    {
        var newScale = new Vector3(localScale.x / 2, localScale.y / 2, localScale.z);
        transform.DOPunchScale(newScale, .2f, 1, 1);
    }

    public void HighlightChain(UnitBehaviour hoveredUnit)
    {
        if (!hoveredUnit) return;

        var chainedUnits = BoardManager.Instance.Chain(hoveredUnit);

        // Get a list of all the units on the board
        var allUnits = BoardManager.Instance.GetAllUnits();

        // Iterate through all the units on the board
        foreach (var unit in allUnits)
        {
            var rend = unit.GetComponent<Renderer>();
            if (rend)
            {
                var mat = rend.material;

                // Check if the unit is not found in the chainedUnits list
                if (!chainedUnits.Contains(unit))
                {
                    // Adjust the HsvSaturation property
                    mat.SetFloat("_HsvSaturation", .2f);
                }
            }
        }
    }

    public void UnhighlightChain()
    {
        var allUnits = BoardManager.Instance.GetAllUnits();

        foreach (var unit in allUnits)
        {
            var rend = unit.GetComponent<Renderer>();
            if (rend)
            {
                var mat = rend.material;
                mat.SetFloat("_HsvSaturation", 1);
            }
        }
    }
}