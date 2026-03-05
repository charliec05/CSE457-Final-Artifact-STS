using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public Effect ManualTargetEffect { get; private set; }
    public List<AutoTargetEffect> OtherEffects { get; private set; }
    public Sprite Image { get; private set; }
    public int Mana { get; private set; }

    public Card(CardData cardData)
    {
        Image = cardData.Image;
        Title = cardData.Title;
        Description = cardData.Description;
        Mana = cardData.Mana;
        ManualTargetEffect = cardData.ManualTargetEffect;
        OtherEffects = cardData.OtherEffects;
    }
}