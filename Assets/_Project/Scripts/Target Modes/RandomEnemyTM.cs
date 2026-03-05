using System.Collections.Generic;

public class RandomEnemyTM : TargetMode
{
    public override List<CombatantView> GetTargets()
    {
        List<EnemyView> enemies = EnemySystem.Instance.Enemies;
        int randomIndex = UnityEngine.Random.Range(0, enemies.Count);
        CombatantView target = enemies[randomIndex];
        return new List<CombatantView>() { target };
    }
}