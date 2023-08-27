using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    public bool isDead = false;
    private float _timeOfSpriteChange;
    private Vector3 _originalScale;
    public UnitBehaviour combatTarget;
    public UnitBehaviour attackedBy;

    private int _spriteCount;
    private Sprite[] _sprites;
    
    private Renderer rend;
    private Material mat;
    private const string HIT = "_HitEffectBlend";
    
    private bool _isAnimating = false;

    // The location that the block is being asked to move to
    public Vector3? targetPosition;
    public bool isMovable;

    public int currentHp;

    public TextMeshProUGUI combatOrderText;
    [SerializeField] private ParticleSystem increaseHealthParticles;
    [SerializeField] private ParticleSystem healParticles;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private GameObject healthUI;
    [SerializeField] private GameObject heartHolder;
    [SerializeField] private GameObject fullHeartObject;
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Sprite fullHeart;
    private List<GameObject> _heartObjects = new List<GameObject>();

    public int _maxHp;
    public int _currentExperience;
    public int _attack;

    private void Awake()
    {
        _blockIcon = GetComponent<SpriteRenderer>();
        


        _currentExperience = 0;
        _attack = unit.attack;
        
    }

    private void Start()
    {
        _maxHp = unit.hp;
        timeSpawned = Time.time;
        _timeOfSpriteChange = Time.time;
        _spriteDuration = .2f;
        _currentSpriteIndex = 0;
        _originalScale = transform.localScale;

        combatOrderText.text = "";
        
        rend = GetComponent<Renderer>();
        if (rend)
        {
            mat = rend.material;
        }
        
        foreach (var effect in unit.effects)
        {
            effect.SetUnit(this);
        }
        
        // Preload all sprites
        var sprites = Resources.LoadAll<Sprite>($"{unit.name}");
        _spriteCount = sprites.Length;
        _sprites = new Sprite[_spriteCount];
        for (int i = 0; i < _spriteCount; i++)
        {
            var sprite = sprites[i]; // name1, name2, etc.
            _sprites[i] = sprite;
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
        if (_sprites.Length <= 0) return;
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
            if (_currentSpriteIndex + 1 >= _spriteCount)
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

    public IEnumerator Attack()
    {
        Debug.Log($"{unit.name} is attempting to attack");
        var combatFinished = false;

        if (!combatTarget)
        {
            Debug.Log($"{unit.name} didn't have a combat target");
            yield break; // If there's no combat target, simply exit the coroutine
        }
        
        foreach(var effect in unit.effects)
        {
            effect.OnAttack();
        }

        BoardManager.Instance.mostRecentlyAttackingUnit = this;
        
        combatOrderText.text = "";

        var originalPos = transform.position;
        mat.SetFloat("_MotionBlurDist", 1);

        transform.DOMove(combatTarget.transform.position, .1f).OnComplete(() =>
        {
            var targets = BoardManager.Instance.Chain(combatTarget);
            Debug.Log($"Number of combat targets: {targets.Count}");
            mat.SetFloat("_MotionBlurDist", 0);
            foreach (var target in targets)
            {
                if (!target) continue;
                target.TakeDamage(unit.attack, this);
            }
            
            // Move the hero back after damaging the enemy

            transform.DOMove(originalPos, .3f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                // Set combatFinished true after all animations are complete
                combatFinished = true;
            });
        });
        
        // Wait until combatFinished becomes true to exit the coroutine
        yield return new WaitUntil(() => combatFinished);
    }

    public void TakeDamage(int amount, UnitBehaviour from)
    {
        Debug.Log($"taking damage from {from.unit.name}");
        attackedBy = from;
        hitParticles.Play();
        
        StartCoroutine(HitEffect());
        currentHp -= amount;

        if (currentHp <= 0)
        {
            Debug.Log($"killed by {from.unit.name}");
            Die();
        }

        healthUI.SetActive(true);
        ShowHearts();
        UpdateHearts();
    }

    private IEnumerator HitEffect()
    {
        if (!mat) yield break;
        
        mat.SetFloat(HIT, 1);

        yield return new WaitForSeconds(.2f);
        
        mat.SetFloat(HIT, 0);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"dead from {attackedBy}");
        foreach(var effect in unit.effects)
        {
            effect.OnDeath();
        }
        
        TurnManager.Instance.RemoveUnit(this);
        // remove block
        
        // play death particles

        deathParticles.Play();
    }

    private void ShowHearts()
    {
        // hearts have already been instantiated
        if (_heartObjects.Count >= 1) return;
        
        for (var i = 0; i < _maxHp; i++)
        {
            _heartObjects.Add(Instantiate(fullHeartObject, heartHolder.transform));
        }
    }

    private void UpdateHearts()
    {
        int startHp = Mathf.Max(0, unit.hp - 1); // Ensure this never goes below 0
        int endHp = Mathf.Max(0, currentHp);     // Ensure this never goes below 0

        for (var i = startHp; i >= endHp; i--)
        {
            var heart = _heartObjects[i];
            var originalPos = heart.transform.position;
            
            heart.GetComponent<Image>().sprite = emptyHeart;

            heart.transform.DOKill();
            heart.transform.DOPunchScale(new Vector3(heart.transform.localScale.x + .2f, heart.transform.localScale.y + .2f), .3f, 1, 1);
            heart.transform.DOPunchPosition(new Vector3(0, originalPos.y + 1, 0), .3f, 1, 1);
        }
    }

    private void UpdateHeartsHeal(int amountToHeal)
    {
        ShowHearts();
        StartCoroutine(HitEffect());
        
        var startingHpIndex = currentHp - 1;
        var maxHeal = _maxHp = 1;

        amountToHeal = Mathf.Max(amountToHeal, maxHeal);

        for (var i = startingHpIndex; i < startingHpIndex + amountToHeal; i++)
        {
            var heart = _heartObjects[i];
            var originalPos = heart.transform.position;
            
            heart.GetComponent<Image>().sprite = fullHeart;

            heart.transform.DOKill();
            heart.transform.DOPunchScale(new Vector3(heart.transform.localScale.x + .2f, heart.transform.localScale.y + .2f), .3f, 1, 1);
            heart.transform.DOPunchPosition(new Vector3(0, originalPos.y + 1, 0), .3f, 1, 1);
        }
    }

    public void Grow()
    {
        var localScale = transform.localScale;
        if (_originalScale == Vector3.zero) _originalScale = localScale;
        
        var newX = localScale.x * 1.5f;
        var newY = localScale.y * 1.5f;
        transform.DOScale(new Vector3(newX, newY, localScale.z), .2f).SetEase(Ease.InOutBounce);
    }

    public void Shrink()
    {
        transform.DOScale(_originalScale, .2f).SetEase(Ease.InOutBounce); 
    }

    public void SetCombatTarget(UnitBehaviour target)
    {
        combatTarget = target;
    }
    
    public void GainExperience(int amount)
    {
        _currentExperience += amount;
    }

    public void Heal(int amount)
    {
        // play animation
        UpdateHeartsHeal(amount);
        
        healParticles.Play();
        
        // gain health
    }

    public void IncreaseHealth(int amount)
    {
        _maxHp += amount;
        currentHp += amount;
        ShowHearts();
        UpdateHearts();

        increaseHealthParticles.Play();
    }
}
