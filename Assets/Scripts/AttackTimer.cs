using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class AttackTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _attackTimerTimeText;
    [SerializeField] private GameObject attackTimerObject;
    
    private int _currActionsUntilAttack;
    public int _totalActionsUntilAttack;
    
    public void Setup(Unit unit)
    {
        _totalActionsUntilAttack = unit.attackTimer;
    }
    
    public void EnableCountdownTimer()
    {
        if (_currActionsUntilAttack < 0) return;

        if (!attackTimerObject.activeSelf)
        {
            attackTimerObject.transform.DOKill();
            var currentTimerSize = attackTimerObject.transform.localScale;
            var newTimerSize = new Vector3(currentTimerSize.x * 1.3f, currentTimerSize.y * 1.3f, 1);
            attackTimerObject.transform.DOPunchScale(newTimerSize, .5f, 1, .3f).SetEase(Ease.OutQuad);
        }
        
        attackTimerObject.SetActive(true);
        _attackTimerTimeText.text = _currActionsUntilAttack.ToString();
        
        if (_currActionsUntilAttack <= 1) StartPulsing();
    }

    public IEnumerator CountDownTimer()
    {
        if (_currActionsUntilAttack < 0) yield break;
 
        var animationFinished = false;

        _currActionsUntilAttack--;
        
        var originalScale = attackTimerObject.transform.localScale;
        _attackTimerTimeText.text = _currActionsUntilAttack.ToString();

        attackTimerObject.transform.DOKill();
        attackTimerObject.transform.DOPunchScale(new Vector3(originalScale.x * 1.2f, originalScale.y * 1.2f + 1, 0), .3f, 1, 1).OnComplete(() =>
        {
            animationFinished = true;
        });
        
        if (_currActionsUntilAttack <= 1) StartPulsing();

        animationFinished = true;
        yield return new WaitUntil(() => animationFinished);
    }
    
    public IEnumerator ResetAttackTimer()
    {
        if (_currActionsUntilAttack < 0) yield break;
        
        StopPulsing();
        
        _attackTimerTimeText.color = Color.white;
        var animationFinished = false;
        var originalScale = attackTimerObject.transform.localScale;
        
        _currActionsUntilAttack = _totalActionsUntilAttack;
        attackTimerObject.transform.DOKill();
        _attackTimerTimeText.text = _currActionsUntilAttack.ToString();
        attackTimerObject.transform.DOPunchScale(new Vector3(originalScale.x * 1.2f, originalScale.y * 1.2f + 1, 0), .3f, 1, 1).OnComplete(() =>
        {
            animationFinished = true;
        });
        
        if (_currActionsUntilAttack <= 1) StartPulsing();
        
        yield return new WaitUntil(() => animationFinished);
    }
    
    public void StartPulsing()
    {
        // _timerAnimationEffects.StartPulsing();
    }

    private void StopPulsing()
    {
        // _timerAnimationEffects.StopPulsing();
    }

    public bool IsReadyToAttack()
    {
        return _currActionsUntilAttack <= _totalActionsUntilAttack;
    }
}
