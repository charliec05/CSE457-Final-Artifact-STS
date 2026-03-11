using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class WaveUI
{
    private static TextMeshProUGUI waveText;
    private static TextMeshProUGUI nameText;
    private static GameObject container;

    public static void Create(int currentWave, int totalWaves, string waveName)
    {
        GameObject canvasObj = new("WaveCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        container = new("WaveContainer");
        container.AddComponent<RectTransform>();
        container.transform.SetParent(canvasObj.transform, false);

        RectTransform containerRT = container.GetComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(0.5f, 1f);
        containerRT.anchorMax = new Vector2(0.5f, 1f);
        containerRT.anchoredPosition = new Vector2(0, -30);
        containerRT.sizeDelta = new Vector2(400, 80);

        Image bg = container.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);

        GameObject waveTextObj = new("WaveText");
        waveTextObj.AddComponent<RectTransform>();
        waveTextObj.transform.SetParent(container.transform, false);

        waveText = waveTextObj.AddComponent<TextMeshProUGUI>();
        waveText.text = $"Wave {currentWave} / {totalWaves}";
        waveText.fontSize = 32;
        waveText.color = Color.white;
        waveText.alignment = TextAlignmentOptions.Center;
        waveText.fontStyle = FontStyles.Bold;

        RectTransform waveRT = waveTextObj.GetComponent<RectTransform>();
        waveRT.anchorMin = new Vector2(0, 0.5f);
        waveRT.anchorMax = new Vector2(1, 1);
        waveRT.offsetMin = new Vector2(0, 0);
        waveRT.offsetMax = Vector2.zero;

        GameObject nameObj = new("WaveName");
        nameObj.AddComponent<RectTransform>();
        nameObj.transform.SetParent(container.transform, false);

        nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = $"~ {waveName} ~";
        nameText.fontSize = 20;
        nameText.color = new Color(0.8f, 0.8f, 0.8f);
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.fontStyle = FontStyles.Italic;

        RectTransform nameRT = nameObj.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0, 0);
        nameRT.anchorMax = new Vector2(1, 0.5f);
        nameRT.offsetMin = Vector2.zero;
        nameRT.offsetMax = Vector2.zero;

        container.transform.localScale = Vector3.zero;
        container.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
    }

    public static void UpdateWave(int currentWave, int totalWaves, string waveName)
    {
        if (waveText != null)
            waveText.text = $"Wave {currentWave} / {totalWaves}";
        if (nameText != null)
            nameText.text = $"~ {waveName} ~";

        if (container != null)
        {
            container.transform.DOScale(1.2f, 0.15f)
                .OnComplete(() => container.transform.DOScale(1f, 0.15f));
        }
    }
}
