using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnitBehaviour : UnitBehaviour
{
    [SerializeField] private AttackTimer _attackTimer;
    
    public override UnitBehaviour Initialize(Unit unit)
    {
        base.Initialize(unit);
        
        _attackTimer.Setup(unitData);

        return this;
    }

    public bool ShouldAttack()
    {
        Debug.Log($"Should {unitData.displayName} attack? Range = {unitData.attackRange}, row = {currentCoordinates.row}. Ready to attack?: {_attackTimer.IsReadyToAttack()}");
        
        
        return _attackTimer.IsReadyToAttack() && unitData.attackRange >= currentCoordinates.row;
    }

    public void TryToShowCountdownTimer()
    {
        if (unitData.attackRange >= currentCoordinates.row) ShowCountdownTimer();
    }

    public void ShowCountdownTimer()
    {
        _attackTimer.EnableCountdownTimer();
    }
    
    public IEnumerator HandleActionTaken()
    {
        if (isDead || unitData.attackRange < currentCoordinates.row) yield break;
        _attackTimer.EnableCountdownTimer();
        yield return StartCoroutine(_attackTimer.CountDownTimer());
    }

    public override IEnumerator Attack(UnitBehaviour attackingTarget)
    {
        if (isDead) yield break;

        var combatFinished = false;
        
        List<EffectState> effectsToRemove = new List<EffectState>();
        foreach (var effectState in effects)
        {
            var isImplemented = effectState.effect.OnAttack(this, attackingTarget, ref attack);
            if (isImplemented)
            {
                Debug.Log($"{effectState.effect.name} implements On Attack");
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
        
        yield return new WaitForSeconds(Timings.TimeBeforeAttack);

        var originalPos = transform.position;
        var slightBackwardPos =
            originalPos -
            (attackingTarget.transform.position - originalPos).normalized *
            0.5f; // Modify this value to control the distance moved backward
        Mat.SetFloat("_MotionBlurDist", 1);

        AudioManager.Instance.PlayWithRandomPitch("whoosh");

        transform.DOKill();
        // First, move slightly backward
        transform.DOMove(slightBackwardPos, 0.05f).OnComplete(() =>
        {
            // After the slight backward motion, charge forward towards the target
            transform.DOMove(attackingTarget.transform.position, .1f).OnComplete(() =>
            {
                var targets = BoardManager.Instance.GetChainedUnits(attackingTarget);
                Debug.Log($"Number of combat targets: {targets.Count}");
                Mat.SetFloat("_MotionBlurDist", 0);
                foreach (var target in targets)
                {
                    if (!target) continue;
                    if (attack >= target.currentHp)
                    {
                        // TODO: execute any OnKill effects
                    }

                    target.TakeDamage(attack, this);
                }

                transform.DOMove(originalPos, .3f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    StartCoroutine(_attackTimer.ResetAttackTimer());
                    combatFinished = true;
                });
            });
        });

        // Wait until combatFinished becomes true to exit the coroutine
        yield return new WaitUntil(() => combatFinished);
    }

    public int GetAttackTimerAmount()
    {
        return _attackTimer._totalActionsUntilAttack;
    }
}