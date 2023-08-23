using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour
{
    private void OnMouseEnter()
    {
        Debug.Log("entered");
        BoardManager.Instance.SetCellSelector(transform.position);
    }
}
