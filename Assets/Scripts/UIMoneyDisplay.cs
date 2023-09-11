using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UIMoneyDisplay : MonoBehaviour
{
    [TitleGroup("Player")]
    [SerializeField]
    private PlayerUI playerUI;

    [TitleGroup("Setup")]
    [SerializeField]
    private TextMeshProUGUI moneyText;

    private void Start()
    {
        playerUI.PlayerHandler.MoneySystem.OnAmountChanged += MoneySystem_OnAmountChanged;

        UpdateMoneyText();
    }

    private void OnDisable()
    {
        playerUI.PlayerHandler.MoneySystem.OnAmountChanged -= MoneySystem_OnAmountChanged;
    }

    private void MoneySystem_OnAmountChanged()
    {
        UpdateMoneyText();
    }

    private void UpdateMoneyText()
    {
        moneyText.text = $"{playerUI.PlayerHandler.MoneySystem.Money}g";
    }
}
