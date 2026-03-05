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

        CardSystem.Instance.Setup(heroData.Deck);
        PerkSystem.Instance.AddPerk(new Perk(perkData));

        WaveUI.Create(WaveManager.CurrentWave + 1, WaveManager.TotalWaves, WaveManager.GetWaveName());

        DrawCardsGA drawCardsGA = new(startingHandSize);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
}
