using System.Collections.Generic;

public class DealDamageGA : GameAction, IHaveCaster
{
    public List<CombatantView> Targets { get; set; }
    public int Amount { get; set; }
    public bool ShowAttackOrb { get; set; }

    public CombatantView Caster { get; private set; }

    public DealDamageGA(int amount, List<CombatantView> targets, CombatantView caster)
    {
        Amount = amount;
        Targets = new(targets);
        Caster = caster;
        ShowAttackOrb = false;
    }
}