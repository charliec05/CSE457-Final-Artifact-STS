using UnityEngine;

public enum CardEffectType
{
    Damage,
    Block,
    Draw
}

public enum CardTargetType
{
    None,
    SingleEnemy,
    AllEnemies,
    Self
}

[System.Serializable]
public class CardEffect
{
    [SerializeField] private CardEffectType effectType;
    [SerializeField] private int value;

    public CardEffectType EffectType => effectType;
    public int Value => value;
}
