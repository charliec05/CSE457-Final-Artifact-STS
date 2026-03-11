using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum StatusEffect
{
    Strength,
    Vulnerable
}

public class CombatantView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float shakeDuration;
    [SerializeField] private float shakeStrength;

    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int Block { get; private set; }

    private readonly Dictionary<StatusEffect, int> statusEffects = new();

    protected void SetupBase(int health, Sprite image)
    {
        MaxHealth = CurrentHealth = health;
        Block = 0;
        statusEffects.Clear();
        spriteRenderer.sprite = image;
        UpdateStatusText();
    }

    public int GetStatus(StatusEffect effect)
    {
        return statusEffects.TryGetValue(effect, out int val) ? val : 0;
    }

    public void ApplyStatus(StatusEffect effect, int stacks)
    {
        if (statusEffects.ContainsKey(effect))
            statusEffects[effect] += stacks;
        else
            statusEffects[effect] = stacks;

        Color flashColor = effect switch
        {
            StatusEffect.Strength => new Color(1f, 0.5f, 0.2f),
            StatusEffect.Vulnerable => new Color(0.9f, 0.2f, 0.9f),
            _ => Color.yellow
        };

        spriteRenderer.DOColor(flashColor, 0.15f)
            .OnComplete(() => spriteRenderer.DOColor(Color.white, 0.15f));

        UpdateStatusText();
    }

    public void TickStatusEffects()
    {
        if (statusEffects.ContainsKey(StatusEffect.Vulnerable))
        {
            statusEffects[StatusEffect.Vulnerable]--;
            if (statusEffects[StatusEffect.Vulnerable] <= 0)
                statusEffects.Remove(StatusEffect.Vulnerable);
        }

        UpdateStatusText();
    }

    public void SetCurrentHealth(int health)
    {
        CurrentHealth = Mathf.Clamp(health, 0, MaxHealth);
        UpdateStatusText();
    }

    public void Damage(int damageAmount)
    {
        int remaining = damageAmount;

        if (Block > 0)
        {
            int absorbed = Mathf.Min(Block, remaining);
            Block -= absorbed;
            remaining -= absorbed;
        }

        if (remaining > 0)
        {
            CurrentHealth -= remaining;
            if (CurrentHealth < 0)
                CurrentHealth = 0;
        }

        transform.DOShakePosition(shakeDuration, shakeStrength);
        UpdateStatusText();
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        spriteRenderer.DOColor(Color.green, 0.15f)
            .OnComplete(() => spriteRenderer.DOColor(Color.white, 0.15f));
        UpdateStatusText();
    }

    public void GainBlock(int amount)
    {
        Block += amount;
        spriteRenderer.DOColor(new Color(0.5f, 0.7f, 1f), 0.15f)
            .OnComplete(() => spriteRenderer.DOColor(Color.white, 0.15f));
        UpdateStatusText();
    }

    public void ResetBlock()
    {
        Block = 0;
        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        string text = "HP: " + CurrentHealth;

        if (Block > 0)
            text += $"  <color=#5599FF>[{Block}]</color>";

        List<string> buffs = new();
        if (GetStatus(StatusEffect.Strength) > 0)
            buffs.Add($"<color=#FF8833>STR {GetStatus(StatusEffect.Strength)}</color>");
        if (GetStatus(StatusEffect.Vulnerable) > 0)
            buffs.Add($"<color=#DD33DD>VUL {GetStatus(StatusEffect.Vulnerable)}</color>");

        if (buffs.Count > 0)
            text += "\n" + string.Join(" ", buffs);

        healthText.text = text;
    }
}
