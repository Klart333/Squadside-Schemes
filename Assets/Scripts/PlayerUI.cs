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

    [Title("End Game")]
    [SerializeField]
    private UIEndGameHandler endGamePanel;

    private Canvas canvas;

    public UITimerDisplay TimerDisplay => timerDisplay;

    private void Start()
    {
        //canvas = GetComponent<Canvas>();
        //canvas.worldCamera = Camera.main;
        //canvas.planeDistance = 1;
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

    public void UpdatePlayerHealth(int clientOwnerID, int health)
    {
        playerHealthHandler.UpdateHealth(clientOwnerID, health);
    }

    public void UpdateAllPlayerHealth(int[] healths)
    {
        for (int i = 0; i < playerHealthHandler.PlayerCount; i++)
        {
            playerHealthHandler.UpdateHealth(i, healths[i]);
        }
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
