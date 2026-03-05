using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchResultSystem : Singleton<MatchResultSystem>
{
    public bool IsMatchOver { get; private set; }

    private readonly float overlayFadeDuration = 0.5f;
    private readonly float elementScaleDuration = 0.4f;
    private readonly float showDelay = 0.8f;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<GameOverGA>(GameOverPerformer);
        ActionSystem.AttachPerformer<GameWonGA>(GameWonPerformer);
        ActionSystem.SubscribeReaction<KillEnemyGA>(CheckVictoryCondition, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<GameOverGA>();
        ActionSystem.DetachPerformer<GameWonGA>();
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(CheckVictoryCondition, ReactionTiming.POST);
    }

    private void CheckVictoryCondition(KillEnemyGA _)
    {
        if (IsMatchOver) return;

        if (EnemySystem.Instance.Enemies.Count == 0)
            ActionSystem.Instance.AddReaction(new GameWonGA());
    }

    private IEnumerator GameOverPerformer(GameOverGA _)
    {
        if (IsMatchOver) yield break;
        IsMatchOver = true;
        yield return new WaitForSeconds(showDelay);
        ShowResultScreen("DEFEAT", new Color(0.9f, 0.2f, 0.2f));
    }

    private IEnumerator GameWonPerformer(GameWonGA _)
    {
        if (IsMatchOver) yield break;
        IsMatchOver = true;
        yield return new WaitForSeconds(showDelay);
        ShowResultScreen("VICTORY!", new Color(1f, 0.84f, 0f));
    }

    #region Programmatic UI
    private void ShowResultScreen(string message, Color titleColor)
    {
        GameObject canvasObj = CreateResultCanvas();
        RectTransform overlay = CreateOverlayPanel(canvasObj.transform);
        CreateTitleText(overlay, message, titleColor);
        CreateRestartButton(overlay);
    }

    private GameObject CreateResultCanvas()
    {
        GameObject canvasObj = new("MatchResultCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        return canvasObj;
    }

    private RectTransform CreateOverlayPanel(Transform parent)
    {
        GameObject overlay = CreateUIChild("Overlay", parent);
        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0);
        overlayImg.DOFade(0.75f, overlayFadeDuration);

        RectTransform rt = overlay.GetComponent<RectTransform>();
        StretchToFill(rt);
        return rt;
    }

    private void CreateTitleText(RectTransform parent, string message, Color color)
    {
        GameObject titleObj = CreateUIChild("ResultTitle", parent);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = message;
        titleText.color = color;
        titleText.fontSize = 96;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;

        RectTransform rt = titleObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.6f);
        rt.anchorMax = new Vector2(0.5f, 0.6f);
        rt.sizeDelta = new Vector2(800, 120);

        titleObj.transform.localScale = Vector3.zero;
        titleObj.transform.DOScale(Vector3.one, elementScaleDuration).SetEase(Ease.OutBack);
    }

    private void CreateRestartButton(RectTransform parent)
    {
        GameObject btnObj = CreateUIChild("RestartButton", parent);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.15f);
        btn.colors = colors;
        btn.onClick.AddListener(RestartMatch);

        RectTransform btnRT = btnObj.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0.35f);
        btnRT.anchorMax = new Vector2(0.5f, 0.35f);
        btnRT.sizeDelta = new Vector2(300, 70);

        btnObj.transform.localScale = Vector3.zero;
        btnObj.transform.DOScale(Vector3.one, elementScaleDuration)
            .SetDelay(0.2f)
            .SetEase(Ease.OutBack);

        GameObject btnTextObj = CreateUIChild("Text", btnObj.transform);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "Restart";
        btnText.color = Color.white;
        btnText.fontSize = 36;
        btnText.alignment = TextAlignmentOptions.Center;
        StretchToFill(btnTextObj.GetComponent<RectTransform>());
    }
    #endregion

    #region UI Helpers
    private static GameObject CreateUIChild(string name, Transform parent)
    {
        GameObject obj = new(name);
        obj.AddComponent<RectTransform>();
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void StretchToFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
    #endregion

    private static void RestartMatch()
    {
        DOTween.KillAll();
        ActionSystem.ClearAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
