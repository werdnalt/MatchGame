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

    // Serialized fields
    [SerializeField] private Image swordSprite;
    [SerializeField] private TextMeshProUGUI attackAmountText;
    [SerializeField] private ParticleSystem increaseHealthParticles;
    [SerializeField] private ParticleSystem healParticles;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private GameObject healthUI;
    [SerializeField] private GameObject heartHolder;
    [SerializeField] private GameObject fullHeartObject;
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private GameObject attackTimerObject;
    [SerializeField] private TextMeshProUGUI attackTimerTimeText;
    [SerializeField] private Image attackTimerSprite;
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

        foreach(var effect in unitData.effects)
        {
            effect.OnAttack(this, attackingTarget);
        }

        BoardManager.Instance.mostRecentlyAttackingUnit = this;
        
        //combatOrderText.text = "";
        //ShowAttackAmount();

        yield return new WaitForSeconds(.75f);

        var originalPos = transform.position;
        mat.SetFloat("_MotionBlurDist", 1);

        AudioManager.Instance.PlayWithRandomPitch("whoosh");
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
                target.TakeDamage(unitData.attack, this);
            }
            
            
            // Move the hero back after damaging the enemy

            transform.DOMove(originalPos, .3f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                // Set combatFinished true after all animations are complete
                combatFinished = true;
                HideAttackAmount();
                StartCoroutine(TurnManager.Instance.TakeTurn());
                if (unitData.tribe == Unit.Tribe.Hero)
                {
                    TurnManager.Instance.CountDownAttackTimers();
                    EnergyManager.Instance.GainEnergy(EnergyManager.Instance.energyGainedPerAttack);
                }
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
        healthUI.SetActive(true);
        ShowHearts();
        UpdateHearts();
        
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
            if (mat) mat.SetFloat("_HsvSaturation", .3f);
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

    private void ShowHearts()
    {
        // If maxHP has increased, instantiate more hearts
        if (_heartObjects.Count < _maxHp)
        {
            // Destroy existing hearts to refresh the display
            foreach (var heart in _heartObjects)
            {
                Destroy(heart);
            }
            _heartObjects.Clear();

            // Create new hearts based on updated _maxHp
            for (var i = 0; i < _maxHp; i++)
            {
                _heartObjects.Add(Instantiate(fullHeartObject, heartHolder.transform));
            }
        }
    }

    public void UpdateHearts()
    {
        // Make sure _heartObjects is populated
        if (_heartObjects.Count == 0) return;

        // Update hearts based on currentHp
        for (var i = 0; i < _maxHp; i++)
        {
            var heart = _heartObjects[i];
            var originalPos = heart.transform.position;
        
            if (i < currentHp)
            {
                // Show filled heart
                heart.GetComponent<Image>().sprite = fullHeart;
            }
            else
            {
                // Show empty heart
                heart.GetComponent<Image>().sprite = emptyHeart;

                // Add animation effects for empty hearts
                heart.transform.DOKill();
                heart.transform.DOPunchScale(new Vector3(heart.transform.localScale.x + .2f, heart.transform.localScale.y + .2f), .3f, 1, 1);
                heart.transform.DOPunchPosition(new Vector3(0, originalPos.y + 1, 0), .3f, 1, 1);
            }
        }
    }

    private void UpdateHeartsHeal(int amountToHeal)
    {
        ShowHearts();
        StartCoroutine(HitEffect());

        var startingHpIndex = currentHp - 1;

        // Ensure you don't heal more than the difference between max HP and current HP.
        var maxHeal = _maxHp - currentHp;
        amountToHeal = Mathf.Min(amountToHeal, maxHeal);

        currentHp += amountToHeal;

        // Ensure we don't exceed the boundaries of _heartObjects array
        int maxIndex = Mathf.Min(startingHpIndex + amountToHeal, _heartObjects.Count);

        for (var i = 0; i < currentHp; i++)
        {
            var heart = _heartObjects[i];
            var originalPos = heart.transform.position;
        
            heart.GetComponent<Image>().sprite = fullHeart;

            heart.transform.DOKill();
            heart.transform.DOPunchScale(new Vector3(heart.transform.localScale.x + .2f, heart.transform.localScale.y + .2f), .3f, 1, 1);
            heart.transform.DOPunchPosition(new Vector3(0, originalPos.y + 1, 0), .3f, 1, 1);
        }
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

    public void ShowAttackAmount()
    {
        attackAmountText.text = attack.ToString();
        swordSprite.enabled = true;
    }

    public void HideAttackAmount()
    {
        attackAmountText.text = "";
        swordSprite.enabled = false;
    }

    public void EnableCountdownTimer()
    {
        if (attackTimer < 0) return;
        
        attackTimerObject.SetActive(true);
        attackTimerTimeText.text = turnsTilAttack.ToString();
    }

    public IEnumerator CountDownTimer()
    {
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

        animationFinished = true;
        yield return new WaitUntil(() => animationFinished);
    }
    
    public IEnumerator ResetAttackTimer()
    {
        if (attackTimer < 0) yield break;
        
        attackTimerTimeText.color = Color.white;
        var animationFinished = false;
        var originalScale = attackTimerObject.transform.localScale;
        
        turnsTilAttack = attackTimer;
        attackTimerTimeText.text = turnsTilAttack.ToString();
        attackTimerObject.transform.DOPunchScale(new Vector3(originalScale.x * 1.2f, originalScale.y * 1.2f + 1, 0), .3f, 1, 1).OnComplete(() =>
        {
            animationFinished = true;
        });
        
        yield return new WaitUntil(() => animationFinished);
    }

    public void AddEffect(Effect effect)
    {
        effects.Add(new EffectState(effect));
        
        Jump();
        deathParticles.Play();
    }
}
