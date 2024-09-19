using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TreasureBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Treasure treasure;

    private LayoutElement _layoutElement;

    private UnitBehaviour _giveTo;

    private void Awake()
    {
        _layoutElement = GetComponent<LayoutElement>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        TreasureManager.Instance.PickupTreasure(this);

        _layoutElement.ignoreLayout = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = GetMouseWorldPos();
    }
    
    private Vector3 GetMouseWorldPos()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        return Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -Camera.main.transform.position.z));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_giveTo)
        {
            _giveTo.AddEffect(treasure.effect);
            Destroy(this.gameObject);
        }
        else
        {
            _layoutElement.ignoreLayout = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        var unitBehaviour = col.gameObject.GetComponent<UnitBehaviour>();

        if (!unitBehaviour) return;
        
        if (col.gameObject.GetComponent<UnitBehaviour>()._unitData.tribe == Unit.Tribe.Hero)
        {
            unitBehaviour.Jump();
            Debug.Log("HERO INTERACTION");
            _giveTo = unitBehaviour;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Exiting");
        _giveTo = null;
    }
}