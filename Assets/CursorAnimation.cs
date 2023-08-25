using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CursorAnimation : MonoBehaviour
{
    public static CursorAnimation Instance;
    
    // The maximum and minimum scales for the pulse animation
    private Vector3 maxScale;
    private Vector3 minScale;

    private Vector3 _startingDragPos;
    
    private BoardManager.Coordinates _startingCell;
    private BoardManager.Coordinates _swappingCell;
    private Vector2 _mousePos;
    private InputAction _mousePositionAction;
    public InputActionAsset inputAction;

    private bool _isSwappingLeft;

    [SerializeField] private Sprite regularSelector;
    [SerializeField] private Sprite swapSelector;

    private SpriteRenderer _spriteRenderer;

    public bool isDragging = false;

    // Duration of each pulse (half of the full cycle since it will be one way only)
    private float pulseDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _mousePositionAction = inputAction.FindAction("MousePosition");
    }

    private void Update()
    {
        if (!isDragging) return;

        _mousePos = _mousePositionAction.ReadValue<Vector2>();

        if (_mousePos.x < _startingDragPos.x && _startingCell.x > 0)
        {
            _swappingCell = new BoardManager.Coordinates(_startingCell.x - 1, _startingCell.y);
            _isSwappingLeft = true;
            _spriteRenderer.sprite = swapSelector;
        }
        else if (_mousePos.x > _startingDragPos.x && _startingCell.x + 1 < BoardManager.Instance.numColumns)
        {
            _swappingCell = new BoardManager.Coordinates(_startingCell.x + 1, _startingCell.y);
            _isSwappingLeft = false;
            _spriteRenderer.sprite = swapSelector;
        }

        SetSwapSelectorPosition();
    }

    private void SetSwapSelectorPosition()
    {
        if (_swappingCell.Equals(new BoardManager.Coordinates(0, 0))) return;

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
        var localScale = transform.localScale;
        
        maxScale = new Vector3(localScale.x + .01f, localScale.x + .01f, localScale.z + .01f);
        minScale = new Vector3(localScale.x, localScale.y, localScale.z);
        StartPulsing();
    }

    void StartPulsing()
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

    public void ChangeColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }

    public void StartDraggingFrom(Vector3 position, BoardManager.Coordinates startingCell)
    {
        if (isDragging) return;
        
        _startingDragPos = position;
        isDragging = true;
        _startingCell = startingCell;
    }

    public void StopDragging()
    {
        ChangeColor(Color.white);
        isDragging = false;
        _isSwappingLeft = false;

        if (!_swappingCell.Equals(new BoardManager.Coordinates(0, 0)))
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
}