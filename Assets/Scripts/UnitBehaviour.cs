using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UnitBehaviour : MonoBehaviour
{
    // Public properties
    public Unit unitData;
    public Coordinates coordinates;
    public bool isDead = false;
    public Vector3? targetPosition;
    public bool cantChain = false;
    public int currentHp;
    public int attack;
    public TextMeshProUGUI combatOrderText;
    public UnitBehaviour combatTarget;
    public UnitBehaviour attackedBy;
    public GameObject defaultAnimatedCharacter;
    public bool isChained;
    public Vector3 characterScale;

    // Serialized fields
    [SerializeField] private Image swordSprite;
    [SerializeField] private TextMeshProUGUI attackAmountText;
    [SerializeField] private ParticleSystem increaseHealthParticles;
    [SerializeField] private ParticleSystem healParticles;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private ParticleSystem attackUpParticles;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] public GameObject healthUI;
    [SerializeField] private GameObject heartHolder;
    [SerializeField] private GameObject fullHeartObject;
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Image fullHeart;
    [SerializeField] private GameObject attackTimerObject;
    [SerializeField] public GameObject skull;
    [SerializeField] private TextMeshProUGUI attackTimerTimeText;
    [SerializeField] private Image attackTimerSprite;
    [SerializeField] private GameObject floatingTextStartingPoint;
    [SerializeField] private GameObject floatingTextEndingPoint;
    [SerializeField] private TextMeshProUGUI floatingText;
    [SerializeField] private TextMeshProUGUI healthAmountText;
    [SerializeField] public GameObject attackUI;
    [SerializeField] private GameObject bonePile;
    [SerializeField] private ParticleSystem smokeParticles;
    public GameObject animatedCharacter;

    // Private fields
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

    private void Start()
    {
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
        }
        else
        {
            animatedCharacter = Instantiate(defaultAnimatedCharacter, transform);
        }

        _animationEffects = animatedCharacter.AddComponent<AnimationEffects>();

        GetAllChildObjects();

        foreach (var effect in unitData.effects)
        {
            effects.Add(new EffectState(effect));
        }
    }
    
    private void GetAllChildObjects()
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

    public UnitBehaviour Initialize(Unit unit)
    {
        unitData = unit;
        
        attack = unitData.attack;
        _maxHp = unitData.hp;
        currentHp = _maxHp;
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
        ShowAndUpdateHealth();
        
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
        
        foreach(var effect in unitData.effects)
        {
            effect.OnDeath(killedBy, this);
        }

        if (unitData.tribe != Unit.Tribe.Hero)
        {
            EnergyManager.Instance.GainEnergy(EnergyManager.Instance.energyGainedPerAttack);
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

    public void EnableCountdownTimer()
    {
        if (attackTimer < 0) return;

        if (!attackTimerObject.activeSelf)
        {
            attackTimerObject.transform.DOKill();
            var currentTimerSize = attackTimerObject.transform.localScale;
            var newTimerSize = new Vector3(currentTimerSize.x * 1.3f, currentTimerSize.y * 1.3f, 1);
            attackTimerObject.transform.DOPunchScale(newTimerSize, .5f, 1, .3f).SetEase(Ease.OutQuad);
        }
        
        attackTimerObject.SetActive(true);
        _timerAnimationEffects = attackTimerObject.GetComponent<AnimationEffects>();
        attackTimerTimeText.text = turnsTilAttack.ToString();
        
        if (attackTimer <= 1) StartPulsing();
    }

    public IEnumerator CountDownTimer()
    {
        if (isDead) yield break;
        
        if (turnsTilAttack < 0) yield break;
        
        Debug.Log("Counting down timers");
        var animationFinished = false;

        turnsTilAttack--;
        
        var originalScale = attackTimerObject.transform.localScale;
        attackTimerTimeText.text = turnsTilAttack.ToString();

        attackTimerObject.transform.DOKill();
        attackTimerObject.transform.DOPunchScale(new Vector3(originalScale.x * 1.2f, originalScale.y * 1.2f + 1, 0), .3f, 1, 1).OnComplete(() =>
        {
            animationFinished = true;
        });
        
        if (turnsTilAttack <= 1) StartPulsing();

        animationFinished = true;
        yield return new WaitUntil(() => animationFinished);
    }
    
    public IEnumerator ResetAttackTimer()
    {
        if (turnsTilAttack < 0) yield break;
        
        StopPulsing();
        
        attackTimerTimeText.color = Color.white;
        var animationFinished = false;
        var originalScale = attackTimerObject.transform.localScale;
        
        turnsTilAttack = attackTimer;
        attackTimerObject.transform.DOKill();
        attackTimerTimeText.text = turnsTilAttack.ToString();
        attackTimerObject.transform.DOPunchScale(new Vector3(originalScale.x * 1.2f, originalScale.y * 1.2f + 1, 0), .3f, 1, 1).OnComplete(() =>
        {
            animationFinished = true;
        });
        
        if (turnsTilAttack <= 1) StartPulsing();
        
        yield return new WaitUntil(() => animationFinished);
    }
    
    public void StartPulsing()
    {
        _timerAnimationEffects.StartPulsing();
    }

    private void StopPulsing()
    {
        _timerAnimationEffects.StopPulsing();
    }

    public void AddEffect(Effect effect)
    {
        effects.Add(new EffectState(effect));
        
        Jump();
        deathParticles.Play();
    }

    public void PlayIncreaseHealthParticles()
    {
        attackUpParticles.Play();
    }

    public void DisplayFloatingText(string textToDisplay, float duration)
    {
        floatingText.transform.position = floatingTextStartingPoint.transform.position;
        floatingText.gameObject.SetActive(true);
        floatingText.text = textToDisplay;

        floatingText.transform.DOMove(floatingTextEndingPoint.transform.position, .75f, false).SetEase(Ease.OutExpo);
        StartCoroutine(TurnOffFloatingText(duration));
    }

    private IEnumerator TurnOffFloatingText(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        floatingText.gameObject.SetActive(false);
    }

    public virtual void ShowAndUpdateHealth()
    {
        healthUI.SetActive(true);

        healthAmountText.text = Mathf.Max(0, currentHp).ToString();

        if (isDead) return;

        if (currentHp > _maxHp)
        {
            healthAmountText.color = new Color32(39, 246, 81, 255);
            fullHeart.fillAmount = 100;
        }

        if (currentHp == _maxHp)
        {
            fullHeart.fillAmount = 100;
        }

        if (currentHp < _maxHp)
        {
            fullHeart.fillAmount = ((float)currentHp / _maxHp);
        }

        // play animation
        healthUI.transform.DOKill();
        if (!_heartSizeSet)
        {
            currentHeartSize = healthUI.transform.localScale;
            _heartSizeSet = true;
        }

        healthUI.transform.DOKill();
        healthUI.transform.localScale = currentHeartSize;
        var newHeartSize = new Vector3(currentHeartSize.x * 1.2f, currentHeartSize.y * 1.2f, 1);
        healthUI.transform.DOPunchScale(newHeartSize, .25f, 0, .3f).SetEase(Ease.OutQuad);
    }

    public void HideHealth()
    {
        if (coordinates.y == 0) return;
        
        healthUI.SetActive(false);
    }

    public void ShowAttack()
    {
        attackUI.SetActive(true);

        if (!attackUI.activeSelf)
        {
            healthUI.transform.DOKill();
            var currentSwordSize = attackUI.transform.localScale;
            var newSwordSize = new Vector3(currentSwordSize.x * 1.2f, currentSwordSize.y * 1.2f, 1);
            attackUI.transform.DOPunchScale(newSwordSize, .25f, 0, .3f).SetEase(Ease.OutQuad);
        }
        
        attackAmountText.text = attack.ToString();
    }

    public void HideAttack()
    {
        attackUI.SetActive(false);
    }

    private void KillTweens()
    {
        if (!animatedCharacter) return;
        
        if (_animationEffects)
        {
            _animationEffects.KillTweens();
        }
        
        if (_timerAnimationEffects)
        {
            _timerAnimationEffects.KillTweens();
        }
        
        animatedCharacter.transform.DOKill();
        transform.DOKill();
        attackTimerObject.transform.DOKill();
        healthUI.transform.DOKill();
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
        _animationEffects.Jello();
    }

    public void Drag(bool isDragging)
    {
        this.isDragging = isDragging;
    }
}
