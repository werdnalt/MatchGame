using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroUnitBehaviour : UnitBehaviour
{
    public override IEnumerator Attack(UnitBehaviour attackingTarget)
    {
        var originalPos = BoardManager.Instance.GetCellPosition(currentCoordinates);
        if (isDead) yield break;

        EventPipe.PlayerAttack();
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
                    
                if (!canChainAttack)
                {
                    targets.Clear();
                    targets.Add(attackingTarget);
                }
                
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

                transform.DOMove(originalPos, .5f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    EventPipe.TakeAction();
                    combatFinished = true;
                });
            });
        });
        
        // Remove effects after the iteration is complete
        foreach (var effectState in effectsToRemove)
        {
            effectState.effect.RemoveEffect(this);
            RemoveEffect(effectState);
        }

        // Wait until combatFinished becomes true to exit the coroutine
        yield return new WaitUntil(() => combatFinished);
    }

    public void GiveTreasure(Treasure treasure)
    {
        // give treasure
    }
}