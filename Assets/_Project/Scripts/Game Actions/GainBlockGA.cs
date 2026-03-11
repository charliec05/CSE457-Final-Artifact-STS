using System.Collections.Generic;

public class GainBlockGA : GameAction
{
    public int Amount { get; set; }
    public List<CombatantView> Targets { get; set; }

    public GainBlockGA(int amount, List<CombatantView> targets)
    {
        Amount = amount;
        Targets = new(targets);
    }
}
