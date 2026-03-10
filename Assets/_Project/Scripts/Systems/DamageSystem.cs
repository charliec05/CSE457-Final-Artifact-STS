using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private GameObject damageVFX;
    [SerializeField] private float damageDuration;

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

        ActionSystem.SubscribeReaction<EnemyTurnGA>(ResetHeroBlock, ReactionTiming.POST);
    }

    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        foreach (CombatantView target in dealDamageGA.Targets)
        {
            target.Damage(dealDamageGA.Amount);
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

    private void ResetHeroBlock(EnemyTurnGA _)
    {
        HeroSystem.Instance.HeroView.ResetBlock();
    }
}
