using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public Unit unit;

    protected float timeSpawned;
    public Type blockType;
    [SerializeField] protected SpriteRenderer _blockIcon;
    protected BoardManager.Coordinates _coordinates;
    public float movementSpeed = 1;

    // The location that the block is being asked to move to
    public Vector3 targetPosition;
    public bool isMovable;

    private void Start()
    {
        timeSpawned = Time.time;
        
        //if (unit.unitSprite) _blockIcon.sprite = unit.unitSprite;
    }

    private void Update()
    {
        if (transform.position != targetPosition)
        {
            Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1)
            {
                transform.position = targetPosition;
            }
        }
    }

    public void Match()
    {
        StartCoroutine(Flash());
    }
    
    private IEnumerator Flash()
    {
        int numFlashes = 5;
        float flashDuration = .1f;
        for (int i = 0; i < numFlashes; i++)
        {
            _blockIcon.material.SetFloat("_HitEffectBlend", 1);
            yield return new WaitForSeconds(flashDuration);
            _blockIcon.material.SetFloat("_HitEffectBlend", 0);
            yield return new WaitForSeconds(flashDuration);
        }
        Destroy(gameObject);
    }

    public void LongFlash()
    {
        StartCoroutine(ILongFlash());
    }
    
    private IEnumerator ILongFlash()
    {
        float flashDuration = .6f;
        _blockIcon.material.SetFloat("_HitEffectBlend", 1);
        yield return new WaitForSeconds(flashDuration);
        _blockIcon.material.SetFloat("_HitEffectBlend", 0);
    }
    
    public void SetCoordinates(BoardManager.Coordinates coordinates)
    {
        _coordinates = coordinates;
    }
}
