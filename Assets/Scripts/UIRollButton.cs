using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRollButton : MonoBehaviour
{
    [TitleGroup("Player")]
    [SerializeField]
    private PlayerUI playerUI;

    [TitleGroup("Setup")]
    [SerializeField]
    private ShopHandler shopHandler;

    [SerializeField]
    private int costToRoll = 2;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        playerUI.PlayerHandler.MoneySystem.OnAmountChanged += CheckHasEnough;
        if (InputManager.Instance?.Roll != null)
        {
            InputManager.Instance.Roll.performed += Roll_performed;
        }

        CheckHasEnough();
    }

    private void Roll_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (playerUI.PlayerHandler.MoneySystem.HasEnough(costToRoll))
        {
            Roll();
        }
    }

    private void OnDisable()
    {
        playerUI.PlayerHandler.MoneySystem.OnAmountChanged -= CheckHasEnough;
    }

    private void CheckHasEnough()
    {
        button.enabled = playerUI.PlayerHandler.MoneySystem.HasEnough(costToRoll);
    }

    public void Roll()
    {
        playerUI.PlayerHandler.MoneySystem.RemoveMoney(costToRoll);
        shopHandler.RefreshShop();
    }
}
