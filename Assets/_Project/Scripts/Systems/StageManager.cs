using System.Collections.Generic;
using UnityEngine;

public static class WaveManager
{
    public const int TotalWaves = 3;

    public static int CurrentWave { get; private set; }
    public static bool IsLastWave => CurrentWave >= TotalWaves - 1;

    private static Sprite slimeSprite;
    private static Sprite redSlimeSprite;

    public static void Initialize(List<EnemyData> baseEnemies)
    {
        CurrentWave = 0;
        slimeSprite = null;
        redSlimeSprite = null;

        foreach (EnemyData enemy in baseEnemies)
        {
            if (slimeSprite == null)
                slimeSprite = enemy.Image;
            else if (redSlimeSprite == null && enemy.Image != slimeSprite)
                redSlimeSprite = enemy.Image;
        }

        if (redSlimeSprite == null)
            redSlimeSprite = slimeSprite;
    }

    public static List<EnemyData> GetEnemiesForCurrentWave()
    {
        return CurrentWave switch
        {
            0 => new List<EnemyData>
            {
                EnemyData.CreateRuntime(slimeSprite, 15, 4),
            },
            1 => new List<EnemyData>
            {
                EnemyData.CreateRuntime(slimeSprite, 18, 5),
                EnemyData.CreateRuntime(redSlimeSprite, 22, 6),
            },
            2 => new List<EnemyData>
            {
                EnemyData.CreateRuntime(redSlimeSprite, 48, 9),
                EnemyData.CreateRuntime(slimeSprite, 24, 7),
            },
            _ => new List<EnemyData>
            {
                EnemyData.CreateRuntime(slimeSprite, 15, 4),
            },
        };
    }

    public static string GetWaveName()
    {
        return CurrentWave switch
        {
            0 => "Slime Encounter",
            1 => "Double Trouble",
            2 => "The Slime King",
            _ => "Unknown",
        };
    }

    public static void AdvanceWave()
    {
        CurrentWave++;
    }

    public static void Reset()
    {
        CurrentWave = 0;
    }
}
