using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitBehaviour : MonoBehaviour
{
    public Unit unit;

    protected float timeSpawned;
    public Type blockType;
    private SpriteRenderer _blockIcon;
    public BoardManager.Coordinates coordinates;
    private float _spriteDuration;
    private int _currentSpriteIndex;
    private bool _isAscending;
    private float _timeOfSpriteChange;
    
    private const int SpriteCount = 3; 
    private Sprite[] _sprites;

    // The location that the block is being asked to move to
    public Vector3? targetPosition;
    public bool isMovable;

    public int currentHp;

    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private GameObject healthUI;
    [SerializeField] private GameObject heartHolder;
    [SerializeField] private GameObject fullHeart;
    [SerializeField] private Sprite emptyHeart;
    private List<GameObject> _heartObjects = new List<GameObject>();

    private void Awake()
    {
        _blockIcon = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        timeSpawned = Time.time;
        _timeOfSpriteChange = Time.time;
        _spriteDuration = .15f;
        _currentSpriteIndex = 0;
        
        // Preload all sprites
        _sprites = new Sprite[SpriteCount];
        for (int i = 0; i < SpriteCount; i++)
        {
            //var sprite = Resources.Load<Sprite>($"{unit.name}{i + 1}"); // name1, name2, etc.
            //_sprites[i] = sprite;
        }
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
        
        AnimateSprite();
    }
    
    private void AnimateSprite()
    {
        // Check if it's time to change the sprite
        if (Time.time - _timeOfSpriteChange < _spriteDuration) return;

        // Set the sprite
        if (_sprites[_currentSpriteIndex] == null)
        {
            return;
        }

        _blockIcon.sprite = _sprites[_currentSpriteIndex];
        _timeOfSpriteChange = Time.time;

        // Ping-Pong logic for sprite animation
        if (_isAscending)
        {
            if (_currentSpriteIndex + 1 >= SpriteCount)
            {
                _isAscending = false;
                _currentSpriteIndex--;
            }
            else
            {
                _currentSpriteIndex++;
            }
        }
        else // you are descending
        {
            if (_currentSpriteIndex <= 0) // If reached the first sprite
            {
                _isAscending = true;
                _currentSpriteIndex++;
            }
            else
            {
                _currentSpriteIndex--;
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

    public UnitBehaviour Initialize(Unit u)
    {
        unit = u;
        _blockIcon.sprite = u.unitSprite;
        currentHp = u.hp;
        return this;
    }

    public void TakeDamage(int amount)
    {
        currentHp -= amount;

        if (currentHp <= 0)
        {
            Die();
        }

        healthUI.SetActive(true);
        ShowHearts();
        UpdateHearts();
    }

    private void Die()
    {
        // remove block
        
        // play death particles

        deathParticles.Play();
    }

    private void ShowHearts()
    {
        // hearts have already been instantiated
        if (_heartObjects.Count >= 1) return;
        
        for (var i = 0; i < unit.hp; i++)
        {
            _heartObjects.Add(Instantiate(fullHeart, heartHolder.transform));
        }
    }

    private void UpdateHearts()
    {
        for (var i = unit.hp - 1; i >= currentHp; i--)
        {
            _heartObjects[i].GetComponent<Image>().sprite = emptyHeart;
        }
    }
    
}
