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
    
    private GameObject _arrowHeadInstance; // The instance of the arrow head
    private GameObject _indicatorInstance;
    
    public enum IndicatorType
    {
        Combat,
        Swap,
        None
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
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.widthMultiplier = 0.1f;
        _lineRenderer.positionCount = 2;

        _arrowHeadInstance = Instantiate(arrowHeadPrefab, Vector3.zero, Quaternion.identity);
        _arrowHeadInstance.SetActive(false); // Initially hidden
        
        _indicatorInstance = Instantiate(indicatorPrefab, Vector3.zero, Quaternion.identity);
        _indicatorInstance.SetActive(false);
    }
    
    void OnEnable()
    {
        _mouseDownAction.started += OnMouseDown;
        _mouseUpAction.canceled += OnMouseUp;
        _mouseMoveAction.performed += OnMouseMove;

        _mouseDownAction.Enable();
        _mouseUpAction.Enable();
        _mouseMoveAction.Enable();
    }
    
    void OnDisable()
    {
        _mouseDownAction.started -= OnMouseDown;
        _mouseUpAction.canceled -= OnMouseUp;
        _mouseMoveAction.performed -= OnMouseMove;

        _mouseDownAction.Disable();
        _mouseUpAction.Disable();
        _mouseMoveAction.Disable();
    }

    public void OnMouseDown(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            
            _mouseStartPos = GetMouseWorldPos();

            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, _mouseStartPos);
            _lineRenderer.SetPosition(1, _mouseStartPos);
            
            _arrowHeadInstance.SetActive(true);
            Cursor.visible = false; // Hide default cursor
            
            _isDrawing = true;
        }
    }

    // New method to handle mouse button up
    public void OnMouseUp(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            _isDrawing = false;
            _lineRenderer.positionCount = 0; // Remove line
            _arrowHeadInstance.SetActive(false);
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

            // Arrow head logic
            _arrowHeadInstance.transform.position = mousePos;
            Vector3 direction = (mousePos - _mouseStartPos).normalized;
            _arrowHeadInstance.transform.right = direction;
            
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

    public void SetIndicator(IndicatorType indicatorType)
    {
        if (!_isDrawing) return;
        
        Debug.Log($"setting indicator to {indicatorType}");
        
        switch (indicatorType)
        {
            case (IndicatorType.Combat):
                _indicatorInstance.SetActive(true);
                _indicatorInstance.GetComponent<SpriteRenderer>().sprite = combatSprite;
                break;
            
            case (IndicatorType.Swap):
                _indicatorInstance.SetActive(true);
                _indicatorInstance.GetComponent<SpriteRenderer>().sprite = swapSprite;
                break;
            
            case (IndicatorType.None):
                _indicatorInstance.SetActive(false);
                break;
        }
    }
}
