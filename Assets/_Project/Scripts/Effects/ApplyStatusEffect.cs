using System.Collections.Generic;
using UnityEngine;

public class ApplyStatusEffect : Effect
{
    [SerializeField] private StatusEffect statusEffect;
    [SerializeField] private int stacks;

    public ApplyStatusEffect() { }

    public ApplyStatusEffect(StatusEffect effect, int amount)
    {
        statusEffect = effect;
        stacks = amount;
    }

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        return new ApplyStatusGA(statusEffect, stacks, targets);
    }
}
