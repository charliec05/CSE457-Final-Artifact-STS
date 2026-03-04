public class DealDamageAction : GameAction
{
    public Enemy Target { get; }
    public int Amount { get; }

    public DealDamageAction(Enemy target, int amount)
    {
        Target = target;
        Amount = amount;
    }
}
