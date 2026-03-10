using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatantView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float shakeDuration;
    [SerializeField] private float shakeStrength;

    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int Block { get; private set; }

    protected void SetupBase(int health, Sprite image)
    {
        MaxHealth = CurrentHealth = health;
        Block = 0;
        spriteRenderer.sprite = image;
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
        healthText.text = text;
    }
}
