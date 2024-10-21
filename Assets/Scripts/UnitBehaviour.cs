using System;
using System.Collections;
using System.Collections.Generic;
using AllIn1SpringsToolkit;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

public abstract class UnitBehaviour : MonoBehaviour
{
    public Coordinates currentCoordinates;
    public bool isDead = false;
    public Vector3? targetPosition;
    public bool cantChain = false;
    public int currentHp;
    public int attack;
    public int armor;
    public int attackRange;
    public TextMeshProUGUI combatOrderText;
    public UnitBehaviour combatTarget;
    public UnitBehaviour attackedBy;
    public GameObject defaultAnimatedCharacter;
    public bool isChained = false;
    public Vector3 characterScale;
    public bool canChainAttack = true;
    public bool shouldBeSentToBack = false;
    public int bonusAttack;

    public bool IsStuck
    {
        get
        {
            var stuck = false;
            foreach (var status in _ongoingStatuses)
            {
                if (status.statusEffect == StatusEffect.Stuck)
                {
                    stuck = true;
                }
            }

            return stuck;
        }
    }

    // Serialized fields
    [SerializeField] private ParticleSystem increaseHealthParticles;
    [SerializeField] private ParticleSystem healParticles;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private ParticleSystem attackUpParticles;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private ParticleSystem poisonParticles;

    [SerializeField] private GameObject attackUI;
    [SerializeField] private TextMeshProUGUI attackAmountText;
    [SerializeField] private HealthUI healthUI;

    [SerializeField] private GameObject damageAmountObject;
    [SerializeField] private TextMeshProUGUI damageAmountText;
    [SerializeField] private GameObject floatingTextStartingPoint;
    [SerializeField] private GameObject floatingTextEndingPoint;
    [SerializeField] private TextMeshProUGUI floatingText;
    [SerializeField] private GameObject bonePile;

    [SerializeField] private ParticleSystem circleStarParticles;

    public GameObject animatedCharacter;

    // Private fields
    public Unit unitData;
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
    private SortingGroup _sortingGroup;
    private CharacterAnimator _characterAnimator;
    private Vector3 _damageTextOriginalScale;
    
    private List<GameObject> animatedCharacterParts = new List<GameObject>();
    private Vector3 currentHeartSize;
    private bool _heartSizeSet;
    private AnimationEffects _animationEffects;
    private AnimationEffects _timerAnimationEffects;
    private TransformSpringComponent _transformSpringComponent;
    private List<Status> _ongoingStatuses = new List<Status>();

    // Public Lists
    public List<EffectState> effects = new List<EffectState>();

    protected bool _isShowingFloatingText;
    private bool _isHolding;
    private bool _hasAnimatedHolding;

    private float _timeEnteredCell = Mathf.Infinity;
    private float _timeUntilChainShown = .5f;
    private float _infoPanelTimeHoverThreshold = .5f;
    
    public Vector3 originalScale;
    private bool scaleSet = false;
    private bool _hasShownPanel = false;
    
    private float _cachedZIndex;

    public enum StatusEffect
    {
        Stuck,
        Poisoned,
        Vulnerable
    }

    public enum Stats
    {
        Health,
        Attack,
        Range,
        Shield
    }

    public class Status
    {
        public StatusEffect statusEffect;
        public UnitBehaviour appliedBy;
        public int actionsLeft;

        public Status(StatusEffect statusEffect, UnitBehaviour appliedBy, int actionsLeft)
        {
            this.statusEffect = statusEffect;
            this.appliedBy = appliedBy;
            this.actionsLeft = actionsLeft;
        }
    }

    private void Awake()
    {
        _transformSpringComponent = GetComponent<TransformSpringComponent>();
        _sortingGroup = GetComponentInChildren<SortingGroup>();
        if (!_sortingGroup)
        {
            _sortingGroup = gameObject.AddComponent<SortingGroup>();
        }

        originalScale = transform.localScale;
        _damageTextOriginalScale = damageAmountObject.transform.localScale;
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
        unitData = unit;

        armor = unit.armor;
        attackRange = unitData.attackRange;
        attack = unitData.attack;
        _maxHp = unitData.hp;
        currentHp = _maxHp;
        cantChain = unit.cantChain;
        
        if (unitData.tribe == Unit.Tribe.NA)
        {
            cantChain = true;
        }

        _rend = GetComponent<Renderer>();
        if (_rend)
        {
            Mat = _rend.material;
        }
        
        // set animated Character
        if (unitData.animatedCharacterPrefab)
        {
            animatedCharacter = Instantiate(unitData.animatedCharacterPrefab, transform);
            _characterAnimator = animatedCharacter.GetComponent<CharacterAnimator>();
        }
        else
        {
            animatedCharacter = Instantiate(defaultAnimatedCharacter, transform);
        }

        _animationEffects = animatedCharacter.AddComponent<AnimationEffects>();

        SetCharacterShader();

        foreach (var effect in unitData.effects)
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
        List<EffectState> effectsToRemove = new List<EffectState>();

        foreach (var effectState in effects)
        {
            if (effectState.isSilenced) continue;
            var isImplemented = effectState.effect.OnHit(attackingUnit, this, ref amount);
            if (isImplemented)
            {
                Debug.Log($"{effectState.effect.name} implements On Hit");
                if (effectState.effect.fromTreasure)
                {
                    EventPipe.UseTreasure(new HeroAndTreasure(this, effectState.effect.fromTreasure));
                }
                if (effectState.isDepleted())
                {
                    effectsToRemove.Add(effectState);  // Add to the list to remove later
                }
            }
        }

        // Remove effects after the iteration is complete
        foreach (var effectState in effectsToRemove)
        {
            RemoveEffect(effectState);
        }

        PunchScale();
        attackedBy = attackingUnit;
        AudioManager.Instance.PlayWithRandomPitch("hit");
        FXManager.Instance.PlayParticles(FXManager.ParticleType.Hit, transform.position);

        var damageAmount = Mathf.Max(amount - armor, 0);
        currentHp -= damageAmount;
        ShowDamageText(damageAmount);

        yield return StartCoroutine(HitEffect());

        if (currentHp <= 0)
        {
            Die(attackingUnit);
        }
        else
        {
            if (shouldBeSentToBack)
            {
                yield return StartCoroutine(BoardManager.Instance.SendUnitToBack(this));
                shouldBeSentToBack = false;
            }
        }
    }

    
    private IEnumerator HitEffect()
    {
        var hitEffectDuration = .25f;
        healthUI.ShowAndUpdateHealth(currentHp, _maxHp);
        //PunchScale();

        // Enable hit effect
        foreach (var part in animatedCharacterParts)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat(HIT, 1);
        }
    
        yield return new WaitForSeconds(hitEffectDuration);

        foreach (var part in animatedCharacterParts)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat(HIT, 0);
        }
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
        List<EffectState> effectsToRemove = new List<EffectState>();
        foreach (var part in animatedCharacterParts)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat(HIT, 1);
        }
        FXManager.Instance.PlayParticles(FXManager.ParticleType.Death, transform.position);
        
        foreach(var effectState in effects)
        {
            if (effectState.isSilenced) continue;
            var isImplemented = effectState.effect.OnDeath(killedBy, this);
            if (isImplemented)
            {
                Debug.Log($"{effectState.effect.name} implements On Death");
                var isDepleted = effectState.isDepleted();
                if (effectState.effect.fromTreasure)
                {
                    EventPipe.UseTreasure(new HeroAndTreasure(this, effectState.effect.fromTreasure));
                }
                if (isDepleted) effectsToRemove.Add(effectState);
            }
        }
        
        // Remove effects after the iteration is complete
        foreach (var effectState in effectsToRemove)
        {
            RemoveEffect(effectState);
        }
        
        effectsToRemove.Clear();
        
        foreach(var effectState in killedBy.effects)
        {
            if (effectState.isSilenced) continue;
            var isImplemented = effectState.effect.OnKill(killedBy, this);
            if (isImplemented)
            {
                Debug.Log($"{effectState.effect.name} implements On Kill");
                var isDepleted = effectState.isDepleted();
                if (effectState.effect.fromTreasure)
                {
                    EventPipe.UseTreasure(new HeroAndTreasure(this, effectState.effect.fromTreasure));
                }
                if (isDepleted) effectsToRemove.Add(effectState);
            }
        }
        
        // Remove effects after the iteration is complete
        foreach (var effectState in effectsToRemove)
        {
            RemoveEffect(effectState);
        }

        if (unitData.tribe != Unit.Tribe.Hero)
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

        StartCoroutine(PlayHealAnimation());
    }

    private IEnumerator PlayHealAnimation()
    {
        // circle heart particles
        circleStarParticles.Play();
        yield return new WaitForSeconds(.35f);
        
        // hit effect
        yield return StartCoroutine(HitEffect());

        // upward heart particles
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
        var effectState = new EffectState(effect);
        effects.Add(effectState);
        effectState.effect.OnObtained(this);
        
        if (effect.fromTreasure)
        {
            DisplayFloatingText($"Got {effect.fromTreasure.name}!", 1);
        }
        StartCoroutine(HitEffect());
        Jump();
        deathParticles.Play();
    }

    public void RemoveEffect(EffectState effectState)
    {
        effects.Remove(effectState);
    }
    
    public void DisplayFloatingText(string textToDisplay, float duration)
    {
        _isShowingFloatingText = true;
        StartCoroutine(ShowFloatingText(textToDisplay, duration));
    }

    private IEnumerator ShowFloatingText(string textToDisplay, float duration)
    {
        floatingText.gameObject.SetActive(true);
        floatingText.transform.position = floatingTextStartingPoint.transform.position;
        floatingText.text = textToDisplay;

        floatingText.transform.DOMove(floatingTextEndingPoint.transform.position, .75f, false).SetEase(Ease.OutExpo);
        yield return StartCoroutine(TurnOffFloatingText(duration));
    }

    private IEnumerator TurnOffFloatingText(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        _isShowingFloatingText = false;

        floatingText.gameObject.SetActive(false);
    }

    private void ShowDamageText(int amount)
    {
        damageAmountObject.SetActive(true);
        damageAmountText.text = amount.ToString();
        damageAmountObject.transform.DOPunchScale(new Vector3(.5f, .5f, .3f), .5f).SetEase(Ease.OutQuad).OnComplete(()=>damageAmountObject.SetActive(false));
    }
    
    public virtual void ShowAndUpdateHealth()
    {
        healthUI.ShowAndUpdateHealth(currentHp, _maxHp);
        attackUI.SetActive(true);
        attackAmountText.text = attack.ToString();
    }

    public void HideHealth()
    {
        if (currentCoordinates.row == Timings.HeroRow || currentCoordinates.row == Timings.EnemyRow || currentHp < _maxHp) return;
        healthUI.HideHealth();
        attackUI.SetActive(false);
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
        // var squash = new Vector3(originalScale.x * 1.4f, originalScale.y * .5f, originalScale.z);
        // _transformSpringComponent.SetCurrentValueScale(squash);
    }

    public void Drag(bool isDragging)
    {
        this.isDragging = isDragging;
    }

    public void PunchScale()
    {
        if (_characterAnimator)
        {
            _characterAnimator.PunchScale();
        }
    }

    public void GrowOverTime(float target, float duration)
    {
        if (!_characterAnimator) return;
        transform.DOScale(target, duration).SetEase(Ease.OutQuad);
    }
    
    public void ShrinkOverTime(float duration)
    {
        if (!_characterAnimator) return;
        transform.DOScale(_characterAnimator.initialScale, duration).SetEase(Ease.InQuad);
    }

    public void Grow(float amount = 1.15f)
    {
        if (_characterAnimator) _characterAnimator.Grow();
    }

    public void Shrink()
    {
        if (_characterAnimator) _characterAnimator.Shrink();
    }

    public void BringSortingToFront()
    {
        _sortingGroup.sortingOrder = 20;
    }

    public void ResetSortingOrder()
    {
        _sortingGroup.sortingOrder = 10 - currentCoordinates.row;
    }

    public void SilenceEffects()
    {
        foreach (var effect in effects)
        {
            effect.isSilenced = true;
        }
    }

    public void AddStatus(Status status)
    {
        _ongoingStatuses.Add(status);

        if (status.statusEffect == StatusEffect.Poisoned)
        {
            poisonParticles.Play();
            foreach (var part in animatedCharacterParts)
            {
                var mat = part.GetComponent<Renderer>().material;
                if (mat) mat.SetFloat("_GradBlend", .6f);
            }
        }
    }

    public void IncreaseAttack(int amount)
    {
        attack += amount;
        ShowAndUpdateHealth();
        attackUpParticles.Play();
    }

    public void CountdownStatuses(int actions)
    {
        var statusesToRemove = new List<Status>();
        foreach (var status in _ongoingStatuses)
        {
            if (status.statusEffect == StatusEffect.Poisoned)
            {
                var poisonAmount = 1 * actions;
                StartCoroutine(ProcessDamage(poisonAmount, status.appliedBy));
            }
            status.actionsLeft -= actions;
            if (status.actionsLeft <= 0) statusesToRemove.Add(status);
        }

        foreach (var status in statusesToRemove)
        {
            if (_ongoingStatuses.Contains(status)) _ongoingStatuses.Remove(status);
        }
    }
}
