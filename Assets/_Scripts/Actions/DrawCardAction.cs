public class DrawCardAction : GameAction
{
    public int Count { get; }

    public DrawCardAction(int count)
    {
        Count = count;
    }
}
