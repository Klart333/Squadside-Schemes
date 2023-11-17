using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public PlayerHandler PlayerHandler { get; set; }

    [Title("Timer")]
    [SerializeField]
    private UITimerDisplay timerDisplay;

    [Title("Shop")]
    [SerializeField]
    private ShopHandler shopHandler;

    [Title("Health")]
    [SerializeField]
    private UIPlayerHealthHandler playerHealthHandler;

    [Title("Unit Inspector")]
    [SerializeField]
    private UIUnitInspector inspectorPanel;

    [Title("End Game")]
    [SerializeField]
    private UIEndGameHandler endGamePanel;

    private Camera cam;

    public UITimerDisplay TimerDisplay => timerDisplay;

    private void Start()
    {
        cam = Camera.main;

        if (InputManager.Instance?.Inspect != null)
        {
            InputManager.Instance.Inspect.performed += Inspect_performed;
        }
    }

    private void Inspect_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            if (hit.collider.gameObject.TryGetComponent(out Unit unit))
            {
                inspectorPanel.gameObject.SetActive(true);
                inspectorPanel.DisplayUnit(unit);
            }
        }
    }

    public void StartRound()
    {
        timerDisplay.StartTimer(GameManager.RoundLength);
        shopHandler.RefreshShop();
    }

    public void EndRoundTimer()
    {
        timerDisplay.StopTimer();
    }

    public void SetupPlayerHealths(int playerCount, ulong[] steamIds)
    {
        List<User> users = new List<User>(steamIds.Length);
        for (int i = 0; i < steamIds.Length; i++)
        {
            users.Add(new User(new Steamworks.CSteamID(steamIds[i])));
        }

        playerHealthHandler.Setup(playerCount, users);
    }

    public void EndGame(bool lost)
    {
        endGamePanel.EndGame(lost);
    }

    public void Overtime()
    {
        timerDisplay.ShowOvertime();
    }

    public void ForceBuyUnitShop(int index)
    {
        shopHandler.ForceBuyUnitShop(index);
    }
}
