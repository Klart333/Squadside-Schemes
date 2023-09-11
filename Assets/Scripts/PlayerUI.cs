using Sirenix.OdinInspector;
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

    public void StartRound()
    {
        timerDisplay.StartTimer(GameManager.RoundLength);
        shopHandler.RefreshShop();
    }

    public void EndRoundTimer()
    {
        timerDisplay.StopTimer();
    }

    public void SetupPlayerHealths(int playerCount)
    {
        playerHealthHandler.Setup(playerCount, new Sprite[] { null, null, null, null });
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
}
