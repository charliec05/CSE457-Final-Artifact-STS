using DG.Tweening;
using TMPro;
using UnityEngine;

public class ManaUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mana;
    [SerializeField] private SpriteRenderer backgroundCircle;

    private readonly Color normalColor = new(0.2f, 0.5f, 1f);
    private readonly Color emptyColor = new(0.9f, 0.2f, 0.2f);

    private GameObject insufficientManaPopup;

    public void UpdateManaDisplay(int currentMana)
    {
        mana.text = currentMana.ToString();

        if (backgroundCircle != null)
            backgroundCircle.DOColor(currentMana > 0 ? normalColor : emptyColor, 0.3f);
    }

    public void ShowInsufficientManaFeedback()
    {
        if (backgroundCircle != null)
        {
            backgroundCircle.transform.DOKill();
            backgroundCircle.transform.DOShakePosition(0.3f, 0.15f, 20);
        }

        ShowFloatingText();
    }

    private void ShowFloatingText()
    {
        if (insufficientManaPopup != null)
            Destroy(insufficientManaPopup);

        GameObject canvasObj = new("InsufficientManaCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject textObj = new("Text");
        RectTransform rt = textObj.AddComponent<RectTransform>();
        textObj.transform.SetParent(canvasObj.transform, false);
        rt.anchorMin = new Vector2(0.5f, 0.35f);
        rt.anchorMax = new Vector2(0.5f, 0.35f);
        rt.sizeDelta = new Vector2(500, 60);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Not Enough Mana!";
        tmp.fontSize = 40;
        tmp.color = new Color(1f, 0.3f, 0.3f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        insufficientManaPopup = canvasObj;

        textObj.transform.localScale = Vector3.zero;
        textObj.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);

        rt.DOAnchorPosY(rt.anchoredPosition.y + 50f, 1.2f).SetEase(Ease.OutQuad);
        tmp.DOFade(0f, 1.2f).SetDelay(0.3f).OnComplete(() => Destroy(canvasObj));
    }
}
