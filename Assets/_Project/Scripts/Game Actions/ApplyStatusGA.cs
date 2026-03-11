using System.Collections.Generic;

public class ApplyStatusGA : GameAction
{
    public StatusEffect Effect { get; set; }
    public int Stacks { get; set; }
    public List<CombatantView> Targets { get; set; }

    public ApplyStatusGA(StatusEffect effect, int stacks, List<CombatantView> targets)
    {
        Effect = effect;
        Stacks = stacks;
        Targets = new(targets);
    }
}
