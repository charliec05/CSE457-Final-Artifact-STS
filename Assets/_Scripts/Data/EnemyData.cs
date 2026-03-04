using UnityEngine;

public enum EnemyIntentType
{
    Attack,
    Defend
}

[System.Serializable]
public class EnemyIntent
{
    [SerializeField] private EnemyIntentType intentType;
    [SerializeField] private int value;

    public EnemyIntentType IntentType => intentType;
    public int Value => value;
}

[CreateAssetMenu(menuName = "Data/Enemy")]
public class EnemyData : ScriptableObject
{
    [field: SerializeField] public string EnemyName { get; private set; }
    [field: SerializeField] public int MaxHealth { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public EnemyIntent[] Intents { get; private set; }
}
