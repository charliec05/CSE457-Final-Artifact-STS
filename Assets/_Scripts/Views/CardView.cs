using UnityEngine;
using TMPro;
using DG.Tweening;

public class CardView : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text mana;
    [SerializeField] private SpriteRenderer imageSR;
    [SerializeField] private GameObject wrapper;
    [SerializeField] private float playZoneMinY = 0f;

    private bool isDragging;
    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;
    private bool playable;

    public Card Card { get; private set; }

    public void Setup(Card card)
    {
        Card = card;
        title.text = card.Title;
        description.text = card.Description;
        mana.text = card.Mana.ToString();
        if (imageSR != null && card.Image != null)
            imageSR.sprite = card.Image;
        SetPlayable(true);
    }

    public void SetPlayable(bool canPlay)
    {
        playable = canPlay;
        if (mana != null)
            mana.color = canPlay ? Color.white : Color.red;
    }

    void OnMouseEnter()
    {
        if (isDragging) return;
        if (wrapper != null) wrapper.SetActive(false);
        if (Card != null && CardViewHoverSystem.Instance != null)
        {
            Vector3 pos = new Vector3(transform.position.x, -2, 0);
            CardViewHoverSystem.Instance.Show(Card, pos);
        }
    }

    void OnMouseExit()
    {
        if (isDragging) return;
        if (CardViewHoverSystem.Instance != null)
            CardViewHoverSystem.Instance.Hide();
        if (wrapper != null) wrapper.SetActive(true);
    }

    void OnMouseDown()
    {
        if (BattleSystem.Instance == null || !BattleSystem.Instance.CanPlayCard(Card)) return;
        isDragging = true;
        if (CardViewHoverSystem.Instance != null)
            CardViewHoverSystem.Instance.Hide();
        if (wrapper != null) wrapper.SetActive(true);
        dragStartPosition = transform.position;
        dragStartRotation = transform.rotation;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;
        var cam = Camera.main;
        if (cam == null) return;
        Vector3 mouse = Input.mousePosition;
        mouse.z = -cam.transform.position.z;
        Vector3 world = cam.ScreenToWorldPoint(mouse);
        world.z = dragStartPosition.z;
        transform.position = world;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        bool inPlayZone = transform.position.y >= playZoneMinY;
        Enemy targetEnemy = null;
        if (inPlayZone && Card.TargetType == CardTargetType.SingleEnemy)
            targetEnemy = GetEnemyUnderMouse();

        if (inPlayZone && (Card.TargetType == CardTargetType.None || Card.TargetType == CardTargetType.Self ||
            Card.TargetType == CardTargetType.AllEnemies || (Card.TargetType == CardTargetType.SingleEnemy && targetEnemy != null && !targetEnemy.IsDead)))
        {
            if (BattleSystem.Instance != null && BattleSystem.Instance.CanPlayCard(Card))
            {
                BattleSystem.Instance.PlayCard(Card, this, targetEnemy);
                return;
            }
        }

        transform.DOMove(dragStartPosition, 0.2f);
        transform.DORotateQuaternion(dragStartRotation, 0.2f);
    }

    private Enemy GetEnemyUnderMouse()
    {
        var cam = Camera.main;
        if (cam == null) return null;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 1000f))
        {
            var enemyView = hit.collider.GetComponent<EnemyView>();
            if (enemyView != null && enemyView.Enemy != null && !enemyView.Enemy.IsDead)
                return enemyView.Enemy;
        }
        return null;
    }
}
