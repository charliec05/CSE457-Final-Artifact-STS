using UnityEngine;
using DG.Tweening;

public class CardViewCreator : Singleton<CardViewCreator>
{
    [SerializeField] private CardView cardViewPrefab;

    public CardView CreateCardView(Vector3 position, Quaternion rotation)
    {
        if (cardViewPrefab == null)
        {
            Debug.LogError("CardViewCreator: cardViewPrefab is NULL (not assigned in Inspector).");
            return null;
        }
        Debug.Log("CreateCardView called");
        CardView cardView = Instantiate(cardViewPrefab, position, rotation);
        Debug.Log("Instantiated: " + cardView.name);
        cardView.transform.localScale = Vector3.zero;
        cardView.transform.DOScale(Vector3.one, 0.15f);
        return cardView;
    }
}
