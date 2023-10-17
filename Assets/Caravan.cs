using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Caravan : MonoBehaviour
{
    public static Caravan Instance;
    private int _currentHp;
    private int _maxHp;
    public int startingHp;
    public Vector3 currentHeartSize;
    private bool _heartSizeSet;

    public Image fullHeart;
    public TextMeshProUGUI healthAmountText;
    public GameObject healthUI;

    [SerializeField] private List<Renderer> caravanPartRenderers;
    private const string HIT = "_HitEffectBlend";

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _maxHp = startingHp;
        _currentHp = startingHp;
    }

    public void TakeDamage(int amount, UnitBehaviour attackedBy)
    {
        StartCoroutine(ProcessDamage(amount, attackedBy));
    }

    private IEnumerator ProcessDamage(int amount, UnitBehaviour attackedBy)
    {
        AudioManager.Instance.PlayWithRandomPitch("hit");
        FXManager.Instance.PlayParticles(FXManager.ParticleType.Hit, transform.position);
        
        _currentHp -= amount;
        
        yield return StartCoroutine(HitEffect());
        
        if (_currentHp <= 0)
        {
            Die(attackedBy);
            yield break;
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
        foreach (var part in caravanPartRenderers)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat(HIT, 1);
        }
    
        yield return new WaitForSeconds(0.2f);

        foreach (var part in caravanPartRenderers)
        {
            var mat = part.GetComponent<Renderer>().material;
            if (mat) mat.SetFloat(HIT, 0);
        }
    
        // Wait for the punch animation to complete
        yield return new WaitUntil(() => punchFinished);
    }
    
    public void ShowAndUpdateHealth()
    {
        healthAmountText.text = Mathf.Max(0, _currentHp).ToString();

        if (_currentHp == _maxHp)
        {
            fullHeart.fillAmount = 100;
        }
    
        if (_currentHp < _maxHp)
        {
            fullHeart.fillAmount = ((float) _currentHp / _maxHp);
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

    private void Die(UnitBehaviour killedBy)
    {
        Debug.Log("Caravan is destroyed");
    }
}
