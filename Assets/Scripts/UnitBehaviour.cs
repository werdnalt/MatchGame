using System;
using System.Collections;
using System.Collections.Generic;
using AllIn1SpringsToolkit;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public abstract class UnitBehaviour : MonoBehaviour
{
    // Public properties
    private Cell _cell;  // Backing field for the cell property

    public Cell cell
    {
        get => _cell;  // Return the backing field
        set
        {
            if (_cell == null)
            {
                _cell = value;
            }
            else
            {
                var newCell = value;

                // The new cell is in the same column, so we drop the cell
                if (newCell.Coordinates.column == _cell.Coordinates.column)
                {
                    var dropDistance = Vector3.Distance(_cell.transform.position, newCell.transform.position);
                    var dropDuration = dropDistance / 15;
                    var newScale = new Vector3(originalScale.x * 1.2f, originalScale.y * 1.2f, originalScale.z * 1.2f);
                    transform.DOMove(newCell.transform.position, dropDuration).SetEase(Ease.OutQuad)
                        .OnComplete(() => _transformSpringComponent.SetCurrentValueScale(newScale));
                }
                // The new cell is in the same row, we are swapping it with a neighbor
                else
                {
                    transform.DOMove(newCell.transform.position, Timings.SwapTime).SetEase(Ease.OutQuad);
                    _cell = value;
                }
            }
        }
    }

    public bool isDead = false;
    public Vector3? targetPosition;
    public bool cantChain = false;
    public int currentHp;
    public int attack;
    public TextMeshProUGUI combatOrderText;
    public UnitBehaviour combatTarget;
    public UnitBehaviour attackedBy;
    public GameObject defaultAnimatedCharacter;
    public bool isChained = false;
    public Vector3 characterScale;

    // Serialized fields
    [SerializeField] private ParticleSystem increaseHealthParticles;
    [SerializeField] private ParticleSystem healParticles;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private ParticleSystem attackUpParticles;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private ParticleSystem smokeParticles;
    
    [SerializeField] private HealthUI healthUI;

    [SerializeField] private GameObject floatingTextStartingPoint;
    [SerializeField] private GameObject floatingTextEndingPoint;
    [SerializeField] private TextMeshProUGUI floatingText;
    [SerializeField] private GameObject bonePile;

    public GameObject animatedCharacter;

    // Private fields
    public Unit _unitData;
    private SpriteRenderer _blockIcon;
    private float _spriteDuration;
    private int _currentSpriteIndex;
    private bool _isAscending;
    private int _spriteCount;
    private Sprite[] _sprites;
    private Renderer _rend;
    protected Material Mat;
    private const string HIT = "_HitEffectBlend";
    private float _elapsedTime = 0f;
    private bool _isAnimating = false;
    public int _maxHp;
    private bool isCountingDown;
    public bool isDragging;
    
    private List<GameObject> animatedCharacterParts = new List<GameObject>();
    private Vector3 currentHeartSize;
    private bool _heartSizeSet;
    private AnimationEffects _animationEffects;
    private AnimationEffects _timerAnimationEffects;
    private TransformSpringComponent _transformSpringComponent;

    // Public Lists
    public List<EffectState> effects = new List<EffectState>();
    
    
    private bool _isHolding;
    private bool _hasAnimatedHolding;

    private float _timeEnteredCell = Mathf.Infinity;
    private float _timeUntilChainShown = .5f;
    private float _infoPanelTimeHoverThreshold = .5f;
    
    private Vector3 originalScale;
    private bool scaleSet = false;
    private bool _hasShownPanel = false;
    
    private float _cachedZIndex;

    private void Awake()
    {
        _transformSpringComponent = GetComponent<TransformSpringComponent>();
    }

    private void SetCharacterShader()
    {
        if (!animatedCharacter) return;

        var spriteRenderer = animatedCharacter.GetComponent<SpriteRenderer>();
        if (spriteRenderer) spriteRenderer.material = Resources.Load<Material>("CharacterShader");
        
        foreach (Transform child in animatedCharacter.transform)
        {
            child.GetComponent<SpriteRenderer>().material = Resources.Load<Material>("CharacterShader");
            animatedCharacterParts.Add(child.gameObject);
        }
    }

    public virtual UnitBehaviour Initialize(Unit unit)
    {
        _unitData = unit;
        
        attack = _unitData.attack;
        _maxHp = _unitData.hp;
        currentHp = _maxHp;
        
        if (_unitData.tribe == Unit.Tribe.NA)
        {
            cantChain = true;
        }

        _rend = GetComponent<Renderer>();
        if (_rend)
        {
            Mat = _rend.material;
        }
        
        // set animated Character
        if (_unitData.animatedCharacterPrefab)
        {
            animatedCharacter = Instantiate(_unitData.animatedCharacterPrefab, transform);
        }
        else
        {
            animatedCharacter = Instantiate(defaultAnimatedCharacter, transform);
        }

        _animationEffects = animatedCharacter.AddComponent<AnimationEffects>();

        SetCharacterShader();

        foreach (var effect in _unitData.effects)
        {
            if (effect == null) continue;
            effects.Add(new EffectState(effect));
        }
        return this;
    }

    public abstract IEnumerator Attack(UnitBehaviour attackingTarget);
    
    public void TakeDamage(int amount, UnitBehaviour attackingUnit)
    {
        StartCoroutine(ProcessDamage(amount, attackingUnit));
    }

    private IEnumerator ProcessDamage(int amount, UnitBehaviour attackingUnit)
    {
        foreach (var effectState in effects)
        {
            var isImplemented = effectState.effect.OnHit(attackingUnit, this, ref amount);
            if (isImplemented)
            {
                Debug.Log($"{effectState.effect.name} implements On Hit");
                var isDepleted = effectState.isDepleted();
            }
        }
        
        attackedBy = attackingUnit;
        AudioManager.Instance.PlayWithRandomPitch("hit");
        FXManager.Instance.PlayParticles(FXManager.ParticleType.Hit, transform.position);
        
        currentHp -= amount;
        
        yield return StartCoroutine(HitEffect());
        
        if (currentHp <= 0)
        {
            Die(attackingUnit);
        }
    }
    
    private IEnumerator HitEffect()
    {        
        healthUI.ShowAndUpdateHealth(currentHp, _maxHp);
        
        //CameraShake.Instance.Shake(.05f);
        var punchDuration = 0.3f;
        var newScale = new Vector3(transform.localScale.x * 1.1f, transform.localScale.y * 1.1f, 1);
    
        // Start the punch scale. OnComplete callback is used to set a flag when the punch animation is finished.
        var punchFinished = false;
        transform.DOKill();
        transform.DOPunchScale(newScale, punchDuration, 1, 1).OnComplete(() => punchFinished = true);
    
        // Enable hit effect
        foreach (var part in animatedCharacterParts)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat(HIT, 1);
        }
    
        yield return new WaitForSeconds(0.2f);

        foreach (var part in animatedCharacterParts)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat(HIT, 0);
        }
    
        // Wait for the punch animation to complete
        yield return new WaitUntil(() => punchFinished);
    }
    
    public void EnableSwapMotionBlur()
    {
        if (!animatedCharacter) return;
        smokeParticles.Play();
        var spriteRenderer = animatedCharacter.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            var mat = spriteRenderer.GetComponent<Renderer>().material;
            if (mat)
            {
                mat.SetFloat("_MotionBlurAngle", .1f);
                mat.SetFloat("_MotionBlurDist", .42f);
            }
        }
        
        foreach (var part in animatedCharacterParts)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat)
            {
                mat.SetFloat("_MotionBlurAngle", .1f);
                mat.SetFloat("_MotionBlurDist", .42f);
            }
        }
    }
    
    public void DisableSwapMotionBlur()
    {
        if (!animatedCharacter) return;
        var spriteRenderer = animatedCharacter.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            var mat = spriteRenderer.GetComponent<Renderer>().material;
            if (mat)
            {
                mat.SetFloat("_MotionBlurAngle", 0f);
                mat.SetFloat("_MotionBlurDist", 0);
            }
        }
        
        foreach (var part in animatedCharacterParts)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat)
            {
                mat.SetFloat("_MotionBlurAngle", 0);
                mat.SetFloat("_MotionBlurDist", 0);
            }
        }
    }

    public void ReduceSaturation()
    {
        var spriteRenderer = animatedCharacter.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            var mat = spriteRenderer.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat("_HsvSaturation", .1f);
        }
        
        foreach (var part in animatedCharacterParts)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat("_HsvSaturation", .1f);
        }
    }

    public void IncreaseSaturation()
    {
        var spriteRenderer = animatedCharacter.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            var mat = spriteRenderer.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat("_HsvSaturation", 1f);
        }
        
        foreach (var part in animatedCharacterParts)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat("_HsvSaturation", 1f);
        }
    }

    private void Die(UnitBehaviour killedBy)
    {
        isDead = true;
        foreach (var part in animatedCharacterParts)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat(HIT, 1);
        }
        FXManager.Instance.PlayParticles(FXManager.ParticleType.Death, transform.position);
        
        foreach(var effect in _unitData.effects)
        {
            effect.OnDeath(killedBy, this);
        }

        if (_unitData.tribe != Unit.Tribe.Hero)
        {
            return;
        }
        
        animatedCharacter.SetActive(false);
        bonePile.SetActive(true);
    }

    public void Jump()
    {
        var endingPos = new Vector3(0, .4f, 0);
        
        transform.DOKill();
        transform.DOPunchPosition(endingPos, .3f, 1, 1).SetEase(Ease.OutQuad);
    }

    public void Heal(int amount)
    {
        var maxHeal = _maxHp - currentHp;
        var amountToHeal = Mathf.Min(amount, maxHeal);

        currentHp += amountToHeal;
        
        ShowAndUpdateHealth();
        
        healParticles.Play();
    }

    public void IncreaseHealth(int amount)
    {
        _maxHp += amount;
        currentHp += amount;
        ShowAndUpdateHealth();
        
        increaseHealthParticles.Play();
    }
    
    public void AddEffect(Effect effect)
    {
        effects.Add(new EffectState(effect));
        
        Jump();
        deathParticles.Play();
    }

    public virtual void ShowAndUpdateHealth()
    {
        healthUI.ShowAndUpdateHealth(currentHp, _maxHp);
    }

    public void HideHealth()
    {
        healthUI.HideHealth();
    }

    private void KillTweens()
    {
        if (!animatedCharacter) return;
        
        // if (_animationEffects)
        // {
        //     _animationEffects.KillTweens();
        // }
        //
        // if (_timerAnimationEffects)
        // {
        //     _timerAnimationEffects.KillTweens();
        // }
        //
        // animatedCharacter.transform.DOKill();
        // transform.DOKill();
        // attackTimerObject.transform.DOKill();
        // healthUI.transform.DOKill();
    }

    public void RemoveSelf()
    {
        KillTweens();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        KillTweens();
        transform.DOKill();
    }
    
    public void ApplyJelloEffect()
    {
        //_animationEffects.Jello();
    }

    public void Drag(bool isDragging)
    {
        this.isDragging = isDragging;
    }
}
