using System.Collections.Generic;

public class HealGA : GameAction
{
    public int Amount { get; set; }
    public List<CombatantView> Targets { get; set; }

    public HealGA(int amount, List<CombatantView> targets)
    {
        Amount = amount;
        Targets = new(targets);
    }
}
