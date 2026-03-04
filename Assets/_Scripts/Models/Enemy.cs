using System;
using UnityEngine;

public class Enemy
{
    public int CurrentHealth { get; private set; }
    public int MaxHealth => data.MaxHealth;
    public int Block { get; private set; }
    public EnemyIntent CurrentIntent { get; private set; }
    public EnemyData Data => data;
    public bool IsDead => CurrentHealth <= 0;

    public event Action OnStatsChanged;
    public event Action<Enemy> OnDeath;

    private readonly EnemyData data;
    private int intentIndex;

    public Enemy(EnemyData enemyData)
    {
        data = enemyData;
        CurrentHealth = data.MaxHealth;
        Block = 0;
        intentIndex = 0;
        if (data.Intents != null && data.Intents.Length > 0)
            CurrentIntent = data.Intents[0];
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        int damageToBlock = Mathf.Min(Block, amount);
        Block -= damageToBlock;
        int damageToHealth = amount - damageToBlock;
        CurrentHealth = Mathf.Max(0, CurrentHealth - damageToHealth);

        OnStatsChanged?.Invoke();
        if (CurrentHealth <= 0)
            OnDeath?.Invoke(this);
    }

    public void GainBlock(int amount)
    {
        if (IsDead) return;
        Block += amount;
        OnStatsChanged?.Invoke();
    }

    public void ClearBlock()
    {
        Block = 0;
        OnStatsChanged?.Invoke();
    }

    public void AdvanceIntent()
    {
        if (data.Intents == null || data.Intents.Length == 0) return;
        intentIndex = (intentIndex + 1) % data.Intents.Length;
        CurrentIntent = data.Intents[intentIndex];
        OnStatsChanged?.Invoke();
    }
}
