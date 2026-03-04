using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public string Title => data.name;

    public string Description => data.Description;

    public Sprite Image => data.Image;

    public int Mana { get; private set; }

    public CardTargetType TargetType => data.TargetType;

    public IReadOnlyList<CardEffect> Effects => data.Effects;

    public CardData Data => data;

    private readonly CardData data;

    public Card(CardData cardData)
    {
        data = cardData;
        Mana = cardData.Mana;
    }
}