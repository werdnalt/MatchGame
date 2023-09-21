using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowLine : MonoBehaviour
{
    public static ArrowLine Instance;
    
    public Sprite combatSprite;
    public Sprite swapSprite;
    
    public InputActionAsset actionAsset;
    public GameObject arrowHeadPrefab; // Prefab for the arrow head
    public GameObject indicatorPrefab;
    
    private LineRenderer _lineRenderer;
    private Vector3 _mouseStartPos;
    private bool _isDrawing = false;
    
    private InputAction _mouseDownAction;
    private InputAction _mouseUpAction;
    private InputAction _mouseMoveAction;

    [SerializeField] private GameObject noSwapIndicator;
    [SerializeField] private GameObject swapIndicator;
    [SerializeField] private GameObject combatIndicator;
    [SerializeField] private GameObject noCombatIndicator;
    
    private GameObject _indicatorInstance;

    public Material lineMat;
    
    public enum IndicatorType
    {
        Combat,
        Swap,
        None,
        NoCombat,
        NoSwap
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        var drawingMap = actionAsset.FindActionMap("Player");
        _mouseDownAction = drawingMap.FindAction("MouseDown");
        _mouseUpAction = drawingMap.FindAction("MouseUp");
        _mouseMoveAction = drawingMap.FindAction("MouseMove");
    }

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        
        _indicatorInstance = Instantiate(indicatorPrefab, Vector3.zero, Quaternion.identity);
        _indicatorInstance.SetActive(false);
    }
    
    void OnEnable()
    {
        // _mouseDownAction.started += OnMouseDown;
        _mouseUpAction.canceled += OnMouseUp;
        _mouseMoveAction.performed += OnMouseMove;

        _mouseDownAction.Enable();
        _mouseUpAction.Enable();
        _mouseMoveAction.Enable();
    }
    
    void OnDisable()
    {
        // _mouseDownAction.started -= OnMouseDown;
        _mouseUpAction.canceled -= OnMouseUp;
        _mouseMoveAction.performed -= OnMouseMove;

        _mouseDownAction.Disable();
        _mouseUpAction.Disable();
        _mouseMoveAction.Disable();
    }
    
    public void StartDrawingLine(Vector3 startPos)
    {
        _mouseStartPos = startPos;

        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.SetPosition(1, GetMouseWorldPos());

        Cursor.visible = false; // Hide default cursor
            
        _isDrawing = true;
    }

    // New method to handle mouse button up
    public void OnMouseUp(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            _isDrawing = false;
            _lineRenderer.positionCount = 0; // Remove line
            _indicatorInstance.SetActive(false);
            Cursor.visible = true; // Unhide default cursor
        }
    }

    // New method to handle mouse move
    public void OnMouseMove(InputAction.CallbackContext context)
    {
        if (_isDrawing)
        {
            Vector3 mousePos = GetMouseWorldPos();
            _lineRenderer.SetPosition(1, mousePos);
            
            Vector3 middlePoint = (_mouseStartPos + mousePos) / 2;
            _indicatorInstance.transform.position = middlePoint;
        }
    }

    // Utility method to get mouse position in world space
    private Vector3 GetMouseWorldPos()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        return Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -Camera.main.transform.position.z));
    }

    public void SetIndicator(IndicatorType indicatorType, Cell cell)
    {
        if (!_isDrawing) return;
        
        Debug.Log($"setting indicator to {indicatorType}");
        
        HideIndicators();

        switch (indicatorType)
        {
            case (IndicatorType.Combat):
                combatIndicator.SetActive(true);
                combatIndicator.transform.position = cell.transform.position;
                break;
            
            case (IndicatorType.Swap):
                swapIndicator.SetActive(true);
                swapIndicator.transform.position = cell.transform.position;
                break;
            
            case (IndicatorType.NoSwap):
                noSwapIndicator.SetActive(true);
                noSwapIndicator.transform.position = cell.transform.position;
                break;
            
            case (IndicatorType.NoCombat):
                noCombatIndicator.SetActive(true);
                noCombatIndicator.transform.position = cell.transform.position;
                break;
            
            case (IndicatorType.None):
                _indicatorInstance.SetActive(false);
                break;
        }
    }

    public void HideIndicators()
    {
        combatIndicator.SetActive(false);
        noCombatIndicator.SetActive(false);
        swapIndicator.SetActive(false);
        noSwapIndicator.SetActive(false);
    }
}
