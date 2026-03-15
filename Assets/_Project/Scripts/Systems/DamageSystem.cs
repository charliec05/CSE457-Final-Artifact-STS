using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private GameObject damageVFX;
    [SerializeField] private float damageDuration;
    [SerializeField] private GameObject attackOrbVFX;

    private WaitForSeconds damageWaitForSeconds;

    private void Awake()
    {
        damageWaitForSeconds = new WaitForSeconds(damageDuration);
    }

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
        ActionSystem.AttachPerformer<HealGA>(HealPerformer);
        ActionSystem.AttachPerformer<GainBlockGA>(GainBlockPerformer);
        ActionSystem.AttachPerformer<ApplyStatusGA>(ApplyStatusPerformer);

        ActionSystem.SubscribeReaction<EnemyTurnGA>(ResetHeroBlock, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(TickHeroStatus, ReactionTiming.POST);
    }

    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        int baseDamage = dealDamageGA.Amount;

        if (dealDamageGA.Caster != null)
            baseDamage += dealDamageGA.Caster.GetStatus(StatusEffect.Strength);

        foreach (CombatantView target in dealDamageGA.Targets)
        {
            int finalDamage = baseDamage;

            if (target.GetStatus(StatusEffect.Vulnerable) > 0)
                finalDamage = Mathf.RoundToInt(finalDamage * 1.5f);

            if (dealDamageGA.ShowAttackOrb && dealDamageGA.Caster != null && attackOrbVFX != null)
            {
                GameObject orbObject = Instantiate(attackOrbVFX, dealDamageGA.Caster.transform.position, Quaternion.identity);
                AttackOrbEffect orbEffect = orbObject.GetComponent<AttackOrbEffect>();

                if (orbEffect != null)
                    orbEffect.target = target.transform;
            }

            target.Damage(finalDamage);
            Instantiate(damageVFX, target.transform.position, Quaternion.identity);
            yield return damageWaitForSeconds;

            if (target.CurrentHealth <= 0)
            {
                if (target is EnemyView enemyView)
                {
                    KillEnemyGA killEnemyGA = new(enemyView);
                    ActionSystem.Instance.AddReaction(killEnemyGA);
                }
                else
                {
                    ActionSystem.Instance.AddReaction(new GameOverGA());
                }
            }
        }
    }

    private IEnumerator HealPerformer(HealGA healGA)
    {
        foreach (CombatantView target in healGA.Targets)
        {
            target.Heal(healGA.Amount);
        }
        yield return damageWaitForSeconds;
    }

    private IEnumerator GainBlockPerformer(GainBlockGA gainBlockGA)
    {
        foreach (CombatantView target in gainBlockGA.Targets)
        {
            target.GainBlock(gainBlockGA.Amount);
        }
        yield return null;
    }

    private IEnumerator ApplyStatusPerformer(ApplyStatusGA applyStatusGA)
    {
        foreach (CombatantView target in applyStatusGA.Targets)
        {
            target.ApplyStatus(applyStatusGA.Effect, applyStatusGA.Stacks);
        }
        yield return null;
    }

    private void ResetHeroBlock(EnemyTurnGA _)
    {
        HeroSystem.Instance.HeroView.ResetBlock();
    }

    private void TickHeroStatus(EnemyTurnGA _)
    {
        HeroSystem.Instance.HeroView.TickStatusEffects();

        foreach (EnemyView enemy in EnemySystem.Instance.Enemies)
        {
            enemy.TickStatusEffects();
        }
    }
}
