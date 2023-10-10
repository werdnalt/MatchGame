using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitBehaviour : MonoBehaviour
{
    // Public properties
    public Unit unitData;
    public bool isDragging;
    public Type blockType;
    public BoardManager.Coordinates coordinates;
    public bool isDead = false;
    public Vector3? targetPosition;
    public bool isMovable;
    public int currentHp;
    public int attack;
    public TextMeshProUGUI combatOrderText;
    public UnitBehaviour combatTarget;
    public UnitBehaviour attackedBy;
    public GameObject defaultAnimatedCharacter;
    public int turnsTilAttack;
    public bool isChained;

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
    [SerializeField] private TextMeshProUGUI attackTimerTimeText;
    [SerializeField] private Image attackTimerSprite;
    [SerializeField] private GameObject floatingTextStartingPoint;
    [SerializeField] private GameObject floatingTextEndingPoint;
    [SerializeField] private TextMeshProUGUI floatingText;
    [SerializeField] private TextMeshProUGUI healthAmountText;
    [SerializeField] public GameObject attackUI;
    public GameObject animatedCharacter;

    // Private fields
    private SpriteRenderer _blockIcon;
    private float _spriteDuration;
    private int _currentSpriteIndex;
    private bool _isAscending;
    private float _timeOfSpriteChange;
    private Vector3 _originalScale;
    private int _spriteCount;
    private Sprite[] _sprites;
    private Renderer rend;
    private Material mat;
    private const string HIT = "_HitEffectBlend";
    private float _elapsedTime = 0f;
    private bool _isAnimating = false;
    private List<GameObject> _heartObjects = new List<GameObject>();
    private int _maxHp;
    private int _currentExperience;
    private bool isCountingDown;
    private int attackTimer;
    private List<GameObject> animatedCharacterParts = new List<GameObject>();

    // Public Lists
    public List<EffectState> effects = new List<EffectState>();
    
    private void Awake()
    {
        _blockIcon = GetComponent<SpriteRenderer>();
        _currentExperience = 0;
    }

    private void Start()
    {
        attackTimer = unitData.attackTimer;
        turnsTilAttack = attackTimer;
        attack = unitData.attack;
        _maxHp = unitData.hp;
        _timeOfSpriteChange = Time.time;
        _spriteDuration = .2f;
        _currentSpriteIndex = 0;
        isDragging = false;

//        combatOrderText.text = "";
        
        rend = GetComponent<Renderer>();
        if (rend)
        {
            mat = rend.material;
        }
        
        // Preload all sprites
        var sprites = Resources.LoadAll<Sprite>($"{unitData.name}");
        _spriteCount = sprites.Length;
        _sprites = new Sprite[_spriteCount];
        for (int i = 0; i < _spriteCount; i++)
        {
            var sprite = sprites[i]; // name1, name2, etc.
            _sprites[i] = sprite;
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
        GetAllChildObjects();

        if (unitData.tribe == Unit.Tribe.Hero)
        {
            ShowAndUpdateHealth();
            ShowAttack();
        }
    }
    
    private void GetAllChildObjects()
    {
        if (!animatedCharacter) return;
        
        foreach (Transform child in animatedCharacter.transform)
        {
            child.GetComponent<SpriteRenderer>().material = Resources.Load<Material>("CharacterShader");
            animatedCharacterParts.Add(child.gameObject);
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
        if (_sprites == null || _sprites.Length <= 0) return;
    
        _elapsedTime += Time.deltaTime; // Time since last frame
    
        if (_elapsedTime >= _spriteDuration)
        {
            // Reset the elapsed time
            _elapsedTime = 0f;

            // Check if the sprite is valid before setting
            if (_sprites[_currentSpriteIndex] == null) return;

            _blockIcon.sprite = _sprites[_currentSpriteIndex];

            // Update sprite index based on animation type
            if (unitData.shouldAnimateLoop)
            {
                // Looping logic
                _currentSpriteIndex = (_currentSpriteIndex + 1) % _sprites.Length;
            }
            else
            {
                // Ping-Pong logic
                if (_isAscending)
                {
                    if (_currentSpriteIndex + 1 >= _sprites.Length)
                    {
                        _isAscending = false;
                        _currentSpriteIndex--;
                    }
                    else
                    {
                        _currentSpriteIndex++;
                    }
                }
                else // Descending
                {
                    if (_currentSpriteIndex <= 0)
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
        unitData = u;
        _blockIcon.sprite = u.unitSprite;
        currentHp = u.hp;
        return this;
    }

    public IEnumerator Attack(UnitBehaviour attackingTarget)
{
    if (isDead) yield break;

    Debug.Log($"{unitData.name} is attempting to attack");
    var combatFinished = false;

    if (!combatTarget)
    {
        Debug.Log($"{unitData.name} didn't have a combat target");
        yield break; // If there's no combat target, simply exit the coroutine
    }

    foreach (var effect in unitData.effects)
    {
        effect.OnAttack(this, attackingTarget);
    }

    BoardManager.Instance.mostRecentlyAttackingUnit = this;

    // combatOrderText.text = "";
    // ShowAttackAmount();

    yield return new WaitForSeconds(.75f);

    var originalPos = transform.position;
    var slightBackwardPos = originalPos - (attackingTarget.transform.position - originalPos).normalized * 0.5f; // Modify this value to control the distance moved backward
    mat.SetFloat("_MotionBlurDist", 1);

    AudioManager.Instance.PlayWithRandomPitch("whoosh");

    // First, move slightly backward
    transform.DOMove(slightBackwardPos, 0.05f).OnComplete(() =>
    {
        // After the slight backward motion, charge forward towards the target
        transform.DOMove(attackingTarget.transform.position, .1f).OnComplete(() =>
        {
            var targets = BoardManager.Instance.Chain(attackingTarget);
            Debug.Log($"Number of combat targets: {targets.Count}");
            mat.SetFloat("_MotionBlurDist", 0);
            foreach (var target in targets)
            {
                if (!target) continue;
                if (attack >= target.currentHp)
                {
                    // TODO: execute any OnKill effects
                }
                target.TakeDamage(attack, this);
            }

            // Move the hero back after damaging the enemy
            transform.DOMove(originalPos, .3f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                // Set combatFinished true after all animations are complete
                combatFinished = true;
                StartCoroutine(TurnManager.Instance.TakeTurn());
                if (unitData.tribe == Unit.Tribe.Hero)
                {
                    TurnManager.Instance.CountDownAttackTimers();
                    EnergyManager.Instance.GainEnergy(EnergyManager.Instance.energyGainedPerAttack);
                }
            });
        });
    });

    // Wait until combatFinished becomes true to exit the coroutine
    yield return new WaitUntil(() => combatFinished);
}


    public void TakeDamage(int amount, UnitBehaviour attackedBy)
    {
        StartCoroutine(ProcessDamage(amount, attackedBy));
    }

    private IEnumerator ProcessDamage(int amount, UnitBehaviour attackedBy)
    {
        foreach (var effect in unitData.effects)
        {
            effect.OnHit(attackedBy, this, ref amount);
        }
        
        this.attackedBy = attackedBy;
        AudioManager.Instance.PlayWithRandomPitch("hit");
        FXManager.Instance.PlayParticles(FXManager.ParticleType.Hit, transform.position);
        
        
        currentHp -= amount;
        
        yield return StartCoroutine(HitEffect());
        
        
        if (currentHp <= 0)
        {
            Die(attackedBy);
            yield break;
        }

        
        if (currentHp > 0 && unitData.tribe != Unit.Tribe.Hero)
        {
            //StartCoroutine(Attack(combatTarget));
        }
    }

    private IEnumerator HitEffect()
    {        
        ShowAndUpdateHealth();
        
        //CameraShake.Instance.Shake(.05f);
        float punchDuration = 0.3f;
        Vector3 newScale = new Vector3(transform.localScale.x * 1.1f, transform.localScale.y * 1.1f, 1);
    
        // Start the punch scale. OnComplete callback is used to set a flag when the punch animation is finished.
        bool punchFinished = false;
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

    public void ReduceSaturation()
    {
        foreach (var part in animatedCharacterParts)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat("_HsvSaturation", .1f);
        }
    }

    public void IncreaseSaturation()
    {
        foreach (var part in animatedCharacterParts)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat("_HsvSaturation", 1f);
        }
    }

    private void Die(UnitBehaviour killedBy)
    {
        isDead = true;
        Debug.Log($"dead from {attackedBy}");
        
        foreach(var effect in unitData.effects)
        {
            effect.OnDeath(killedBy, this);
        }
        
        TurnManager.Instance.RemoveUnit(this);
        // remove block
        
        // play death particles

        deathParticles.Play();
    }

    public void Jump()
    {
        var endingPos = new Vector3(0, .4f, 0);
        
        transform.DOKill();
        transform.DOPunchPosition(endingPos, .3f, 1, 1).SetEase(Ease.OutQuad);
    }

    public void Grow()
    {
        transform.DOKill();
        _originalScale = transform.localScale;
        
        var newX = transform.localScale.x * 1.5f;
        var newY = transform.localScale.y * 1.5f;
        transform.DOScale(new Vector3(newX, newY, transform.localScale.z), .2f).SetEase(Ease.InOutBounce);
    }

    public void Shrink()
    {
        transform.DOKill();
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
        attackTimerTimeText.text = turnsTilAttack.ToString();
        
        if (attackTimer <= 1) StartPulsing();
    }

    public IEnumerator CountDownTimer()
    {
        if (isDead) yield break;
        
        if (attackTimer < 0) yield break;
        
        Debug.Log("Counting down timers");
        var animationFinished = false;

        turnsTilAttack--;
        
        var originalScale = attackTimerObject.transform.localScale;
        attackTimerTimeText.text = turnsTilAttack.ToString();

        attackTimerObject.transform.DOPunchScale(new Vector3(originalScale.x * 1.2f, originalScale.y * 1.2f + 1, 0), .3f, 1, 1).OnComplete(() =>
        {
            animationFinished = true;
        });
        
        if (attackTimer <= 1) StartPulsing();

        animationFinished = true;
        yield return new WaitUntil(() => animationFinished);
    }
    
    public IEnumerator ResetAttackTimer()
    {
        if (attackTimer < 0) yield break;
        
        StopPulsing();
        
        attackTimerTimeText.color = Color.white;
        var animationFinished = false;
        var originalScale = attackTimerObject.transform.localScale;
        
        turnsTilAttack = attackTimer;
        attackTimerTimeText.text = turnsTilAttack.ToString();
        attackTimerObject.transform.DOPunchScale(new Vector3(originalScale.x * 1.2f, originalScale.y * 1.2f + 1, 0), .3f, 1, 1).OnComplete(() =>
        {
            animationFinished = true;
        });
        
        if (attackTimer <= 1) StartPulsing();
        
        yield return new WaitUntil(() => animationFinished);
    }
    
    public void StartPulsing()
    {
        var localScale = attackTimerObject.transform.localScale;
        var maxScale = new Vector3(localScale.x * 1.3f, localScale.x * 1.3f, localScale.z);
        var minScale = new Vector3(localScale.x, localScale.y, localScale.z);

        var pulseDuration = .3f;
    
        attackTimerObject.transform.DOScale(maxScale, pulseDuration)
            .SetEase(Ease.InOutSine) // Smooth in and out
            .SetId(attackTimerObject.GetInstanceID()) // Set unique ID for this tween
            .OnComplete(() =>
            {
                attackTimerObject.transform.DOScale(minScale, pulseDuration)
                    .SetEase(Ease.InOutSine)
                    .SetId(attackTimerObject.GetInstanceID()) // Set unique ID for this tween
                    .OnComplete(StartPulsing); // Recursively call to create a loop
            });
    }

    private void StopPulsing()
    {
        // Only kill the tween that has the specific ID for this GameObject
        DOTween.Kill(attackTimerObject.GetInstanceID());
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

    public void ShowAndUpdateHealth()
    {
        healthUI.SetActive(true);
        healthAmountText.text = currentHp.ToString();

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
        var currentHeartSize = healthUI.transform.localScale;
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
}
