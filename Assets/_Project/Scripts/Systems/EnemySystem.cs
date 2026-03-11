using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    public List<EnemyView> Enemies => enemyBoardView.EnemyViews;

    [SerializeField] private EnemyBoardView enemyBoardView;
    [SerializeField] private float attackXMoveAmount;
    [SerializeField] private float attackMoveDuration;
    [SerializeField] private float attackReturnDuration;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);
        ActionSystem.AttachPerformer<SpawnNextWaveGA>(SpawnNextWavePerformer);
    }

    public void Setup(List<EnemyData> enemyDataList)
    {
        foreach (EnemyData enemyData in enemyDataList)
        {
            enemyBoardView.AddEnemy(enemyData);
        }
    }

    #region Performers
    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        foreach (EnemyView enemy in enemyBoardView.EnemyViews)
        {
            AttackHeroGA attackHeroGA = new(enemy);
            ActionSystem.Instance.AddReaction(attackHeroGA);
        }

        yield return null;
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        EnemyView attacker = attackHeroGA.Attacker;
        Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, attackMoveDuration);
        yield return tween.WaitForCompletion();
        attacker.transform.DOMoveX(attacker.transform.position.x + 1f, attackReturnDuration);
        DealDamageGA dealDamageGA = new(attacker.AttackPower, new() { HeroSystem.Instance.HeroView }, attackHeroGA.Caster);
        ActionSystem.Instance.AddReaction(dealDamageGA);
    }

    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        yield return enemyBoardView.RemoveEnemy(killEnemyGA.EnemyView);
    }

    private IEnumerator SpawnNextWavePerformer(SpawnNextWaveGA _)
    {
        WaveManager.AdvanceWave();

        int wave = WaveManager.CurrentWave;
        WaveUI.UpdateWave(wave + 1, WaveManager.TotalWaves, WaveManager.GetWaveName());

        yield return new WaitForSeconds(0.8f);

        List<EnemyData> waveEnemies = WaveManager.GetEnemiesForCurrentWave();
        foreach (EnemyData enemyData in waveEnemies)
        {
            enemyBoardView.AddEnemy(enemyData);
        }

        yield return new WaitForSeconds(0.3f);
    }
    #endregion
}
