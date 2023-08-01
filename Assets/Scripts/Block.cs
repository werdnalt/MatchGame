using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public Unit unit;

    protected float timeSpawned;
    public Type blockType;
    private SpriteRenderer _blockIcon;
    public BoardManager.Coordinates coordinates;

    // The location that the block is being asked to move to
    public Vector3? targetPosition;
    public bool isMovable;

    private int _currentHp;

    [SerializeField] private GameObject healthUI;
    [SerializeField] private TextMeshProUGUI healthAmountText;

    private void Awake()
    {
        _blockIcon = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        timeSpawned = Time.time;
    }

    private void Update()
    {
        if (targetPosition.HasValue && transform.position != targetPosition)
        { 
            transform.position = Vector3.MoveTowards(transform.position, targetPosition.Value, 10 * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition.Value) < 0.1)
            {
               transform.position = targetPosition.Value;
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

    public void Initialize(Unit u)
    {
        unit = u;
        _blockIcon.sprite = u.unitSprite;
        _currentHp = u.hp;
    }

    public void TakeDamage(int amount)
    {
        if (amount >= _currentHp)
        {
            Die();
        }

        else
        {
            _currentHp -= amount;
        }

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        healthUI.SetActive(true);
        healthAmountText.text = _currentHp.ToString();
    }

    private void Die()
    {
        // remove block
    }
}
