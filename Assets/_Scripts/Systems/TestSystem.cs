using UnityEngine;

public class TestSystem : MonoBehaviour
{
    [SerializeField] private HandView handView;

    private void Update()
    {
        Debug.Log("TestSystem UPDATE running");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("SPACE detected");
            CardView cardView = CardViewCreator.Instance.CreateCardView(
                transform.position,
                Quaternion.identity
            );

            StartCoroutine(handView.AddCard(cardView));
        }
    }
}