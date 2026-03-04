using UnityEngine;
using TMPro;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text blockText;

    public void Refresh(BattleSystem battle)
    {
        if (battle == null) return;
        if (healthText != null)
            healthText.text = $"{battle.CurrentPlayerHealth}/{battle.MaxPlayerHealth}";
        if (blockText != null)
        {
            blockText.text = battle.CurrentBlock > 0 ? battle.CurrentBlock.ToString() : "";
            blockText.gameObject.SetActive(battle.CurrentBlock > 0);
        }
    }
}
