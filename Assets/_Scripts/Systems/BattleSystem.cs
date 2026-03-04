using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Central combat manager. Setup: create empty GameObject, add this + CardActionSystem + CardViewCreator + CardViewHoverSystem.
/// Assign starterDeck (CardData assets), enemyEncounter (EnemyData), enemyViewSlots (EnemyView in scene), handView, playerView, battleUI, drawPileSpawnPoint.
/// For cards: set CardData TargetType and Effects in Inspector (e.g. Fireball: SingleEnemy + Effect Damage 4).
/// </summary>
public class BattleSystem : Singleton<BattleSystem>
{
    [Header("Player")]
    [SerializeField] private int maxPlayerHealth = 70;
    [SerializeField] private int maxEnergyPerTurn = 3;
    [SerializeField] private int cardsToDrawPerTurn = 5;

    [Header("Deck")]
    [SerializeField] private CardData[] starterDeck;
    [SerializeField] private Transform drawPileSpawnPoint;

    [Header("Views")]
    [SerializeField] private HandView handView;
    [SerializeField] private PlayerView playerView;
    [SerializeField] private BattleUI battleUI;
    [SerializeField] private EnemyView[] enemyViewSlots;
    [SerializeField] private EnemyData[] enemyEncounter;

    private int currentPlayerHealth;
    private int currentBlock;
    private int currentEnergy;
    private bool isPlayerTurn;
    private bool battleEnded;

    private readonly List<Card> drawPile = new();
    private readonly List<Card> discardPile = new();
    private readonly List<Card> hand = new();
    private readonly List<(CardView view, Card card)> handViews = new();
    private readonly List<Enemy> enemies = new();

    public int CurrentPlayerHealth => currentPlayerHealth;
    public int MaxPlayerHealth => maxPlayerHealth;
    public int CurrentBlock => currentBlock;
    public int CurrentEnergy => currentEnergy;
    public int MaxEnergyPerTurn => maxEnergyPerTurn;
    public bool IsPlayerTurn => isPlayerTurn;
    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
    public IReadOnlyList<Enemy> Enemies => enemies;

    public event Action OnPlayerStatsChanged;
    public event Action OnBattleWon;
    public event Action OnBattleLost;

    private void Start()
    {
        RegisterPerformers();
        InitBattle();
    }

    private void OnDestroy()
    {
        CardActionSystem.DetachPerformer<PlayCardAction>();
        CardActionSystem.DetachPerformer<DealDamageAction>();
        CardActionSystem.DetachPerformer<GainBlockAction>();
        CardActionSystem.DetachPerformer<DrawCardAction>();
    }

    private void RegisterPerformers()
    {
        CardActionSystem.AttachPerformer<PlayCardAction>(PlayCardPerformer);
        CardActionSystem.AttachPerformer<DealDamageAction>(DealDamagePerformer);
        CardActionSystem.AttachPerformer<GainBlockAction>(GainBlockPerformer);
        CardActionSystem.AttachPerformer<DrawCardAction>(DrawCardPerformer);
    }

    private void InitBattle()
    {
        battleEnded = false;
        currentPlayerHealth = maxPlayerHealth;
        currentBlock = 0;
        drawPile.Clear();
        discardPile.Clear();
        hand.Clear();
        handViews.Clear();
        enemies.Clear();

        if (starterDeck != null)
        {
            foreach (var data in starterDeck)
            {
                if (data != null)
                    drawPile.Add(new Card(data));
            }
        }

        Shuffle(drawPile);

        if (enemyEncounter != null && enemyViewSlots != null)
        {
            for (int i = 0; i < enemyEncounter.Length && i < enemyViewSlots.Length; i++)
            {
                if (enemyEncounter[i] != null && enemyViewSlots[i] != null)
                {
                    var enemy = new Enemy(enemyEncounter[i]);
                    enemies.Add(enemy);
                    enemyViewSlots[i].Setup(enemy);
                }
                else if (i < enemyViewSlots.Length && enemyViewSlots[i] != null)
                {
                    enemyViewSlots[i].Setup(null);
                }
            }
            for (int i = enemyEncounter?.Length ?? 0; i < enemyViewSlots?.Length; i++)
            {
                if (enemyViewSlots[i] != null)
                    enemyViewSlots[i].gameObject.SetActive(false);
            }
        }

        RefreshAllUI();
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        if (battleEnded) return;
        currentBlock = 0;
        currentEnergy = maxEnergyPerTurn;
        isPlayerTurn = true;
        RefreshAllUI();
        StartCoroutine(DrawCardsCoroutine(cardsToDrawPerTurn));
    }

    public void EndPlayerTurn()
    {
        if (!isPlayerTurn || battleEnded || CardActionSystem.Instance.isPerforming) return;
        isPlayerTurn = false;
        StartCoroutine(EnemyTurnCoroutine());
    }

    private IEnumerator EnemyTurnCoroutine()
    {
        DiscardHand();
        RefreshAllUI();
        yield return new WaitForSeconds(0.3f);

        foreach (var enemy in enemies.ToArray())
        {
            if (enemy.IsDead) continue;
            if (enemy.CurrentIntent == null) continue;

            switch (enemy.CurrentIntent.IntentType)
            {
                case EnemyIntentType.Attack:
                    PlayerTakeDamage(enemy.CurrentIntent.Value);
                    break;
                case EnemyIntentType.Defend:
                    enemy.GainBlock(enemy.CurrentIntent.Value);
                    break;
            }

            enemy.AdvanceIntent();
            RefreshAllUI();
            yield return new WaitForSeconds(0.5f);
        }

        foreach (var enemy in enemies)
            enemy.ClearBlock();

        if (battleEnded) yield break;
        yield return new WaitForSeconds(0.3f);
        StartPlayerTurn();
    }

    private void PlayerTakeDamage(int amount)
    {
        int toBlock = Mathf.Min(currentBlock, amount);
        currentBlock -= toBlock;
        int toHealth = amount - toBlock;
        currentPlayerHealth = Mathf.Max(0, currentPlayerHealth - toHealth);
        OnPlayerStatsChanged?.Invoke();
        RefreshAllUI();
        if (currentPlayerHealth <= 0)
        {
            battleEnded = true;
            OnBattleLost?.Invoke();
        }
    }

    public void PlayerGainBlock(int amount)
    {
        currentBlock += amount;
        OnPlayerStatsChanged?.Invoke();
        RefreshAllUI();
    }

    public bool CanPlayCard(Card card)
    {
        return isPlayerTurn && !battleEnded && !CardActionSystem.Instance.isPerforming && currentEnergy >= card.Mana;
    }

    public void PlayCard(Card card, CardView cardView, Enemy targetEnemy)
    {
        if (!CanPlayCard(card)) return;
        if (card.TargetType == CardTargetType.SingleEnemy && targetEnemy == null) return;
        if (card.TargetType == CardTargetType.SingleEnemy && (targetEnemy == null || targetEnemy.IsDead)) return;

        int handIndex = handViews.FindIndex(t => t.view == cardView);
        if (handIndex < 0) return;

        var action = new PlayCardAction(card, cardView, targetEnemy, handIndex);

        if (card.Effects != null)
        {
            foreach (var effect in card.Effects)
            {
                switch (effect.EffectType)
                {
                    case CardEffectType.Damage:
                        if (card.TargetType == CardTargetType.SingleEnemy && targetEnemy != null && !targetEnemy.IsDead)
                            action.PerformReactions.Add(new DealDamageAction(targetEnemy, effect.Value));
                        else if (card.TargetType == CardTargetType.AllEnemies)
                        {
                            foreach (var e in enemies)
                            {
                                if (!e.IsDead)
                                    action.PerformReactions.Add(new DealDamageAction(e, effect.Value));
                            }
                        }
                        break;
                    case CardEffectType.Block:
                        action.PerformReactions.Add(new GainBlockAction(effect.Value));
                        break;
                    case CardEffectType.Draw:
                        action.PerformReactions.Add(new DrawCardAction(effect.Value));
                        break;
                }
            }
        }

        CardActionSystem.Instance.Perform(action, () =>
        {
            RefreshAllUI();
            CheckWinCondition();
        });
    }

    private void CheckWinCondition()
    {
        if (battleEnded) return;
        bool allDead = true;
        foreach (var e in enemies)
        {
            if (!e.IsDead) { allDead = false; break; }
        }
        if (allDead)
        {
            battleEnded = true;
            OnBattleWon?.Invoke();
        }
    }

    private IEnumerator PlayCardPerformer(PlayCardAction action)
    {
        currentEnergy -= action.Card.Mana;
        RefreshAllUI();

        handViews.RemoveAt(action.HandIndex);
        hand.Remove(action.Card);
        discardPile.Add(action.Card);
        handView.RemoveCard(action.CardView);
        if (action.CardView != null && action.CardView.gameObject != null)
            Destroy(action.CardView.gameObject);

        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator DealDamagePerformer(DealDamageAction action)
    {
        if (action.Target == null || action.Target.IsDead) yield break;
        action.Target.TakeDamage(action.Amount);
        var view = GetEnemyView(action.Target);
        if (view != null)
            view.PlayHitEffect();
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator GainBlockPerformer(GainBlockAction action)
    {
        PlayerGainBlock(action.Amount);
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator DrawCardPerformer(DrawCardAction action)
    {
        yield return DrawCardsCoroutine(action.Count);
    }

    private IEnumerator DrawCardsCoroutine(int count)
    {
        Vector3 spawnPos = drawPileSpawnPoint != null ? drawPileSpawnPoint.position : Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            var card = DrawOneCard();
            if (card == null) break;
            hand.Add(card);
            var view = CardViewCreator.Instance.CreateCardView(card, spawnPos, Quaternion.identity);
            handViews.Add((view, card));
            yield return handView.AddCard(view);
        }
        RefreshAllUI();
    }

    private Card DrawOneCard()
    {
        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0) return null;
            foreach (var c in discardPile)
                drawPile.Add(c);
            discardPile.Clear();
            Shuffle(drawPile);
        }
        if (drawPile.Count == 0) return null;
        var card = drawPile[drawPile.Count - 1];
        drawPile.RemoveAt(drawPile.Count - 1);
        return card;
    }

    private void Shuffle(List<Card> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public void DiscardHand()
    {
        for (int i = handViews.Count - 1; i >= 0; i--)
        {
            var (view, card) = handViews[i];
            handViews.RemoveAt(i);
            hand.Remove(card);
            discardPile.Add(card);
            handView.RemoveCard(view);
            if (view != null && view.gameObject != null)
                Destroy(view.gameObject);
        }
    }

    private EnemyView GetEnemyView(Enemy enemy)
    {
        if (enemyViewSlots == null) return null;
        foreach (var ev in enemyViewSlots)
        {
            if (ev != null && ev.Enemy == enemy) return ev;
        }
        return null;
    }

    public Enemy GetEnemyAt(int index)
    {
        if (index < 0 || index >= enemies.Count) return null;
        return enemies[index];
    }

    public void RefreshAllUI()
    {
        OnPlayerStatsChanged?.Invoke();
        playerView?.Refresh(this);
        battleUI?.Refresh(this);
        foreach (var cv in handViews)
            cv.view?.SetPlayable(CanPlayCard(cv.card));
    }
}
