using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILevelButton : MonoBehaviour
{
    [TitleGroup("Player")]
    [SerializeField]
    private PlayerUI playerUI;

    [TitleGroup("Setup")]
    [SerializeField]
    private TextMeshProUGUI levelText;

    [SerializeField]
    private int costToBuyXP = 4;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        SetLevelText();

        playerUI.PlayerHandler.MoneySystem.OnAmountChanged += CheckHasEnough;
        playerUI.PlayerHandler.LevelSystem.OnLevelUp += SetLevelText;

        CheckHasEnough();
    }

    private void OnDisable()
    {
        playerUI.PlayerHandler.MoneySystem.OnAmountChanged -= CheckHasEnough;
    }

    private void CheckHasEnough()
    {
        button.enabled = playerUI.PlayerHandler.MoneySystem.HasEnough(costToBuyXP);
    }

    public void BuyXP()
    {
        if (playerUI.PlayerHandler.LevelSystem.CurrentLevel >= 10)
        {
            Debug.Log("Max level");
            return;
        }

        playerUI.PlayerHandler.MoneySystem.RemoveMoney(costToBuyXP);
        playerUI.PlayerHandler.LevelSystem.AddXP(4);
    }

    private void SetLevelText()
    {
        levelText.text = $"Lv. {playerUI.PlayerHandler.LevelSystem.CurrentLevel}";
    }
}
