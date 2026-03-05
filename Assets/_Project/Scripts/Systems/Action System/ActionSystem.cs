using System;
using System.Collections;
using System.Collections.Generic;

public class ActionSystem : Singleton<ActionSystem>
{
    public bool IsPerforming { get; private set; } = false;

    private static readonly Dictionary<Type, List<Action<GameAction>>> preSubscribers = new();
    private static readonly Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();
    private static readonly Dictionary<Type, List<Action<GameAction>>> postSubscribers = new();

    private List<GameAction> reactions = null;

    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
    {
        Type type = typeof(T);
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action);

        if (performers.ContainsKey(type))
            performers[type] = wrappedPerformer;
        else
            performers.Add(type, wrappedPerformer);
    }

    public static void DetachPerformer<T>() where T : GameAction
    {
        Type type = typeof(T);

        if (performers.ContainsKey(type))
            performers.Remove(type);
    }

    public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        Dictionary<Type, List<Action<GameAction>>> subscribers = GetSubscribersForTiming(timing);

        void wrappedReaction(GameAction action) => reaction((T)action);

        if (subscribers.ContainsKey(typeof(T)))
            subscribers[typeof(T)].Add(wrappedReaction);
        else
        {
            subscribers.Add(typeof(T), new());
            subscribers[typeof(T)].Add(wrappedReaction);
        }
    }

    public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        Dictionary<Type, List<Action<GameAction>>> subscribers = GetSubscribersForTiming(timing);

        if (subscribers.ContainsKey(typeof(T)))
        {
            void wrappedReaction(GameAction action) => reaction((T)action);
            subscribers[typeof(T)].Remove(wrappedReaction);
        }
    }

    public void Perform(GameAction action, Action OnPerformFinished = null)
    {
        if (IsPerforming) return;

        IsPerforming = true;

        StartCoroutine(Flow(action, () =>
        {
            IsPerforming = false;
            OnPerformFinished?.Invoke();
        }));
    }

    public void AddReaction(GameAction gameAction)
    {
        reactions?.Add(gameAction);
    }

    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null)
    {
        reactions = action.PreReactions;
        PerformSubscribers(action, preSubscribers);
        yield return PerformReactions();

        reactions = action.PerformReactions;
        yield return PerformPerformer(action);
        yield return PerformReactions();

        reactions = action.PostReactions;
        PerformSubscribers(action, postSubscribers);
        yield return PerformReactions();

        OnFlowFinished?.Invoke();
    }

    private IEnumerator PerformPerformer(GameAction action)
    {
        Type type = action.GetType();

        if (performers.TryGetValue(type, out Func<GameAction, IEnumerator> performer))
            yield return performer(action);
    }

    private IEnumerator PerformReactions()
    {
        foreach (GameAction reaction in reactions)
        {
            yield return Flow(reaction);
        }
    }

    private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs)
    {
        Type type = action.GetType();

        if (subs.TryGetValue(type, out List<Action<GameAction>> actions))
        {
            foreach (Action<GameAction> sub in actions)
            {
                sub(action);
            }
        }
    }

    public static void ClearAll()
    {
        preSubscribers.Clear();
        performers.Clear();
        postSubscribers.Clear();
    }

    private static Dictionary<Type, List<Action<GameAction>>> GetSubscribersForTiming(ReactionTiming timing)
    {
        return timing == ReactionTiming.PRE ? preSubscribers : postSubscribers;
    }
}
