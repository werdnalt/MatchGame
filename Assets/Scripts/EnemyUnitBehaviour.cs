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
        
        _attackTimer.Setup(_unitData);
        
        EventPipe.OnActionTaken += HandleActionTaken;
        return this;
    }

    public bool ShouldAttack()
    {
        return _attackTimer.IsReadyToAttack();
    }

    private void HandleActionTaken()
    {
        if (isDead) return;

        if (cell.Coordinates.row - _unitData.attackRange <= Timings.HeroRow)
        {
            StartCoroutine(_attackTimer.CountDownTimer());
        }
    }

    public void EnableCountdownTimer()
    {
        _attackTimer.EnableCountdownTimer();
    }

    public override IEnumerator Attack(UnitBehaviour attackingTarget)
    {
        if (isDead) yield break;

        var combatFinished = false;

        if (!combatTarget)
        {
            yield break; // If there's no combat target, simply exit the coroutine
        }

        foreach (var effectState in effects)
        {
            effectState.effect.OnAttack(this, attackingTarget, ref attack);
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
}