using System.Collections.Generic;
using UnityEngine;

public class GainBlockEffect : Effect
{
    [SerializeField] private int blockAmount;

    public GainBlockEffect() { }
    public GainBlockEffect(int amount) { blockAmount = amount; }

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        return new GainBlockGA(blockAmount, targets);
    }
}
