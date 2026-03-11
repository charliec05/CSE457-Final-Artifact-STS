using System.Collections.Generic;

public class SelfTM : TargetMode
{
    public override List<CombatantView> GetTargets()
    {
        return new List<CombatantView> { HeroSystem.Instance.HeroView };
    }
}
