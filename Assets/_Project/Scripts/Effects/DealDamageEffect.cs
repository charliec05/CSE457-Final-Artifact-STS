using System.Collections.Generic;
using UnityEngine;

public class DealDamageEffect : Effect
{
    [SerializeField] private int damageAmount;
    [SerializeField] private bool showAttackOrb;

    public DealDamageEffect() { }
    public DealDamageEffect(int amount)
    {
        damageAmount = amount;
        showAttackOrb = false;
    }

    public DealDamageEffect(int amount, bool showOrb)
    {
        damageAmount = amount;
        showAttackOrb = showOrb;
    }

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        DealDamageGA dealDamageGA = new DealDamageGA(damageAmount, targets, caster);
        dealDamageGA.ShowAttackOrb = showAttackOrb;
        return dealDamageGA;
    }
}