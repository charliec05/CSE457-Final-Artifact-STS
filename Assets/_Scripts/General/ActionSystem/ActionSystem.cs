using System;
using System.Collections;
using System.Collections.Generic;

public class CardActionSystem : Singleton<CardActionSystem>
{
    private List<GameAction> reactions = null;

    public bool isPerforming { get; private set; } = false;

    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();
    private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();

    // Mapping between original delegates and wrapped delegates
    private static Dictionary<Type, Dictionary<Delegate, Action<GameAction>>> preWrapperMap = new();
    private static Dictionary<Type, Dictionary<Delegate, Action<GameAction>>> postWrapperMap = new();

    // Queue of immediate reactions to process (LinkedList keeps insertion order)
    private LinkedList<GameAction> immediateReactions = new LinkedList<GameAction>();

    // Whether the immediate reaction queue is currently executing
    // (prevents re-entering immediate execution while already running)
    private bool isInImmediateReaction = false;

    public void Perform(GameAction action, Action OnPerformFinished = null)
    {
        if (isPerforming) return;

        isPerforming = true;

        // Reset state in case the previous process was interrupted unexpectedly
        isInImmediateReaction = false;

        StartCoroutine(Flow(action, () =>
        {
            isPerforming = false;
            OnPerformFinished?.Invoke();
        }));
    }

    public void AddReaction(GameAction gameAction)
    {
        reactions?.Add(gameAction);
    }

    private IEnumerator Flow(GameAction action, Action OnFlowfinished = null)
    {
        reactions = action.PreReactions;
        PerformSubscribers(action, preSubs);
        yield return PerformReactions();

        reactions = action.PerformReactions;
        yield return PerformPerformer(action);
        yield return PerformReactions();

        reactions = action.PostReactions;
        PerformSubscribers(action, postSubs);
        yield return PerformReactions();

        OnFlowfinished?.Invoke();
    }

    private IEnumerator PerformReactions()
    {
        foreach (var reaction in reactions)
        {
            yield return PerformImmediateReactions();
            yield return Flow(reaction);
        }
        yield return PerformImmediateReactions();
    }

    private IEnumerator PerformPerformer(GameAction action)
    {
        Type type = action.GetType();
        if (performers.ContainsKey(type))
        {
            yield return PerformImmediateReactions();
            yield return performers[type](action);
        }
        yield return PerformImmediateReactions();
    }

    /// <summary>
    /// Execute the immediate reaction queue
    /// </summary>
    private IEnumerator PerformImmediateReactions()
    {
        if (isInImmediateReaction) yield break;
        isInImmediateReaction = true;

        while (immediateReactions.Count > 0)
        {
            var reaction = immediateReactions.First.Value;
            immediateReactions.RemoveFirst();

            yield return Flow(reaction);
        }

        isInImmediateReaction = false;
    }

    private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs)
    {
        Type currentType = action.GetType();

        // Traverse the current type and all base types
        while (currentType != null && currentType != typeof(object))
        {
            if (subs.TryGetValue(currentType, out var subscribers))
            {
                var subscribersCopy = subscribers.ToArray();
                foreach (var sub in subscribersCopy)
                {
                    sub(action);
                }
            }

            currentType = currentType.BaseType;
        }
    }

    /// <summary>
    /// Add an immediate reaction action (enqueue it)
    /// </summary>
    public void AddImmediateReaction(GameAction gameAction)
    {
        immediateReactions.AddLast(gameAction);
    }

    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
    {
        Type type = typeof(T);
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action);

        if (performers.ContainsKey(type))
            performers[type] = wrappedPerformer;
        else
            performers.Add(type, wrappedPerformer);
    }

    /// <summary>
    /// Remove performer
    /// </summary>
    public static void DetachPerformer<T>() where T : GameAction
    {
        Type type = typeof(T);
        if (performers.ContainsKey(type))
            performers.Remove(type);
    }

    /// <summary>
    /// Remove performer and all reactions subscribed to it
    /// </summary>
    public static void DetachPerformerAndSubscribers<T>() where T : GameAction
    {
        Type type = typeof(T);
        DetachPerformer<T>();

        if (preSubs.ContainsKey(type))
        {
            preSubs[type].Clear();
        }
        if (preWrapperMap.ContainsKey(type))
        {
            preWrapperMap[type].Clear();
        }

        if (postSubs.ContainsKey(type))
        {
            postSubs[type].Clear();
        }
        if (postWrapperMap.ContainsKey(type))
        {
            postWrapperMap[type].Clear();
        }
    }

    public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        Dictionary<Type, Dictionary<Delegate, Action<GameAction>>> wrapperMap =
            timing == ReactionTiming.PRE ? preWrapperMap : postWrapperMap;

        Type type = typeof(T);

        Action<GameAction> wrappedReaction = action => reaction((T)action);

        if (!subs.ContainsKey(type))
        {
            subs[type] = new List<Action<GameAction>>();
            wrapperMap[type] = new Dictionary<Delegate, Action<GameAction>>();
        }

        subs[type].Add(wrappedReaction);

        wrapperMap[type][reaction] = wrappedReaction;
    }

    public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        Dictionary<Type, Dictionary<Delegate, Action<GameAction>>> wrapperMap =
            timing == ReactionTiming.PRE ? preWrapperMap : postWrapperMap;

        Type type = typeof(T);

        if (!subs.ContainsKey(type) || !wrapperMap.ContainsKey(type))
            return;

        var wrapperDict = wrapperMap[type];

        if (wrapperDict.TryGetValue(reaction, out var wrappedReaction))
        {
            subs[type].Remove(wrappedReaction);
            wrapperDict.Remove(reaction);
        }
    }

    public void AbortAllActions()
    {
        StopAllCoroutines();

        reactions?.Clear();

        isPerforming = false;
    }
}
