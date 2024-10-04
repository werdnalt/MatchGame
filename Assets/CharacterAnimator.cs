using System;
using System.Collections;
using System.Collections.Generic;
using AllIn1SpringsToolkit;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public Vector3 initialScale;

    private TransformSpringComponent _transformSpringComponent;

    private void Awake()
    {
        initialScale = transform.localScale;
        _transformSpringComponent = GetComponent<TransformSpringComponent>();
    }

    public void PunchScale()
    {
        var newScale = new Vector3(initialScale.x * 1.5f, initialScale.y * 1.5f, initialScale.z * 1.5f);
        if (_transformSpringComponent)
        {
            _transformSpringComponent.SetCurrentValueScale(newScale);
        }
    }

    public void Grow()
    {
        var newScale = new Vector3(initialScale.x * 1.3f, initialScale.y * 1.3f, initialScale.z * 1.3f);
        if (_transformSpringComponent)
        {
            _transformSpringComponent.SetTargetScale(newScale);
        }
    }

    public void Shrink()
    {
        if (_transformSpringComponent)
        {
            _transformSpringComponent.SetTargetScale(initialScale);
        }
    }
    
}
