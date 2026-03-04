using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class EnemyView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text blockText;
    [SerializeField] private TMP_Text intentText;

    public Enemy Enemy { get; private set; }

    public void Setup(Enemy enemy)
    {
        if (Enemy != null)
        {
            Enemy.OnStatsChanged -= Refresh;
            Enemy.OnDeath -= OnEnemyDeath;
        }

        Enemy = enemy;
        if (Enemy == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        if (spriteRenderer != null && Enemy.Data.Sprite != null)
            spriteRenderer.sprite = Enemy.Data.Sprite;
        if (nameText != null)
            nameText.text = Enemy.Data.EnemyName;

        Enemy.OnStatsChanged += Refresh;
        Enemy.OnDeath += OnEnemyDeath;
        Refresh();
    }

    private void OnEnemyDeath(Enemy _)
    {
        Enemy.OnStatsChanged -= Refresh;
        Enemy.OnDeath -= OnEnemyDeath;
        gameObject.SetActive(false);
    }

    private void Refresh()
    {
        if (Enemy == null) return;
        if (healthText != null)
            healthText.text = $"{Enemy.CurrentHealth}/{Enemy.MaxHealth}";
        if (blockText != null)
        {
            blockText.text = Enemy.Block > 0 ? Enemy.Block.ToString() : "";
            blockText.gameObject.SetActive(Enemy.Block > 0);
        }
        if (intentText != null && Enemy.CurrentIntent != null)
        {
            switch (Enemy.CurrentIntent.IntentType)
            {
                case EnemyIntentType.Attack:
                    intentText.text = $"⚔ {Enemy.CurrentIntent.Value}";
                    break;
                case EnemyIntentType.Defend:
                    intentText.text = $"🛡 {Enemy.CurrentIntent.Value}";
                    break;
                default:
                    intentText.text = "";
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        if (Enemy != null)
        {
            Enemy.OnStatsChanged -= Refresh;
            Enemy.OnDeath -= OnEnemyDeath;
        }
    }

    public void PlayHitEffect()
    {
        transform.DOShakePosition(0.2f, 0.1f);
    }
}
