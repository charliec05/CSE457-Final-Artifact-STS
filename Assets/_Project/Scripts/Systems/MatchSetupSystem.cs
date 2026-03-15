using System.Collections.Generic;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private HeroData heroData;
    [SerializeField] private PerkData perkData;
    [SerializeField] private List<EnemyData> enemyDataList;
    [SerializeField] private int startingHandSize = 5;

    private void Start()
    {
        if (MatchResultSystem.Instance == null)
            new GameObject("MatchResultSystem").AddComponent<MatchResultSystem>();

        WaveManager.Initialize(enemyDataList);

        HeroSystem.Instance.Setup(heroData);

        List<EnemyData> waveEnemies = WaveManager.GetEnemiesForCurrentWave();
        EnemySystem.Instance.Setup(waveEnemies);

        List<CardData> deck = BuildDeck();
        CardSystem.Instance.Setup(deck);
        PerkSystem.Instance.AddPerk(new Perk(perkData));

        WaveUI.Create(WaveManager.CurrentWave + 1, WaveManager.TotalWaves, WaveManager.GetWaveName());

        DrawCardsGA drawCardsGA = new(startingHandSize);
        ActionSystem.Instance.Perform(drawCardsGA);
    }

    private List<CardData> BuildDeck()
    {
        Sprite attackImg = GetCardSprite(0);
        Sprite spellImg = GetCardSprite(1);

        List<CardData> deck = new();

        // 4x Strike — 1 mana, deal 6 damage (manual target)
        for (int i = 0; i < 4; i++)
            deck.Add(CardData.CreateRuntime(
                "Strike", "Deal 6 damage", 1, attackImg,
                new DealDamageEffect(6, true), null));

        // 3x Defend — 1 mana, gain 5 block (self)
        for (int i = 0; i < 3; i++)
            deck.Add(CardData.CreateRuntime(
                "Defend", "Gain 5 Block", 1, spellImg,
                null, new List<AutoTargetEffect> {
                    new(new SelfTM(), new GainBlockEffect(5))
                }));

        // 2x Heal — 1 mana, restore 5 HP (self)
        for (int i = 0; i < 2; i++)
            deck.Add(CardData.CreateRuntime(
                "Heal", "Restore 5 HP", 1, spellImg,
                null, new List<AutoTargetEffect> {
                    new(new SelfTM(), new HealEffect(5))
                }));

        // 1x Power Strike — 2 mana, deal 14 damage (manual target)
        deck.Add(CardData.CreateRuntime(
            "Power Strike", "Deal 14 damage", 2, attackImg,
            new DealDamageEffect(14, true), null));

        // 1x Cleave — 1 mana, deal 4 damage to ALL enemies
        deck.Add(CardData.CreateRuntime(
            "Cleave", "Deal 4 damage to ALL enemies", 1, attackImg,
            null, new List<AutoTargetEffect> {
                new(new AllEnemiesTM(), new DealDamageEffect(4, true))
            }));

        // 1x Whirlwind — 2 mana, deal 8 damage to ALL enemies
        deck.Add(CardData.CreateRuntime(
            "Whirlwind", "Deal 8 damage to ALL enemies", 2, attackImg,
            null, new List<AutoTargetEffect> {
                new(new AllEnemiesTM(), new DealDamageEffect(8, true))
            }));

        // 1x Flex — 0 mana, gain 2 Strength (self)
        deck.Add(CardData.CreateRuntime(
            "Flex", "Gain 2 Strength", 0, spellImg,
            null, new List<AutoTargetEffect> {
                new(new SelfTM(), new ApplyStatusEffect(StatusEffect.Strength, 2))
            }));

        // 1x Bash — 2 mana, deal 8 damage + apply 2 Vulnerable to all enemies
        deck.Add(CardData.CreateRuntime(
            "Bash", "Deal 8 damage\nAll enemies: 2 Vulnerable", 2, attackImg,
            new DealDamageEffect(8, true), new List<AutoTargetEffect> {
                new(new AllEnemiesTM(), new ApplyStatusEffect(StatusEffect.Vulnerable, 2))
            }));

        // 1x Battle Trance — 0 mana, draw 2 cards
        deck.Add(CardData.CreateRuntime(
            "Battle Trance", "Draw 2 cards", 0, spellImg,
            null, new List<AutoTargetEffect> {
                new(new NoTM(), new DrawCardsEffect(2))
            }));

        return deck;
    }

    private Sprite GetCardSprite(int index)
    {
        if (heroData.Deck != null && heroData.Deck.Count > index)
            return heroData.Deck[index].Image;
        if (heroData.Deck != null && heroData.Deck.Count > 0)
            return heroData.Deck[0].Image;
        return null;
    }
}
