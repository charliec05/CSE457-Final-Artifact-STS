using System.Collections.Generic;

public class PlayCardAction : GameAction
{
    public Card Card { get; }
    public CardView CardView { get; }
    public Enemy TargetEnemy { get; }
    public int HandIndex { get; }

    public PlayCardAction(Card card, CardView cardView, Enemy targetEnemy, int handIndex)
    {
        Card = card;
        CardView = cardView;
        TargetEnemy = targetEnemy;
        HandIndex = handIndex;
    }
}
