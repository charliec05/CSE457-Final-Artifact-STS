using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text drawPileText;
    [SerializeField] private TMP_Text discardPileText;
    [SerializeField] private Button endTurnButton;

    private void Start()
    {
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }

    private void OnEndTurnClicked()
    {
        if (BattleSystem.Instance != null)
            BattleSystem.Instance.EndPlayerTurn();
    }

    public void Refresh(BattleSystem battle)
    {
        if (battle == null) return;
        if (energyText != null)
            energyText.text = $"{battle.CurrentEnergy}/{battle.MaxEnergyPerTurn}";
        if (drawPileText != null)
            drawPileText.text = battle.DrawPileCount.ToString();
        if (discardPileText != null)
            discardPileText.text = battle.DiscardPileCount.ToString();
        if (endTurnButton != null)
            endTurnButton.interactable = battle.IsPlayerTurn && CardActionSystem.Instance != null && !CardActionSystem.Instance.isPerforming;
    }
}
