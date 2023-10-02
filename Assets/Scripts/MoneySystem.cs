using System;
using UnityEngine;

public class MoneySystem : MonoBehaviour
{
    public event Action OnAmountChanged;

    public PlayerHandler PlayerHandler { get; set; }

    public int Money { get; private set; } = 0;
    public int MaxInterest { get; set; } = 5;

    public int WinStreak { get; set; }
    public int LoseStreak { get; set; }

    public bool HasEnough(int cost)
    {
        return Money >= cost;
    }

    public void AddMoney(int amount)
    {
        Money += amount;

        OnAmountChanged?.Invoke();
    }

    public void RemoveMoney(int amount)
    {
        AddMoney(-Mathf.Abs(amount));
    }

    public void StartRound()
    {
        int money = 5;
        money += GetStreakGold(WinStreak > LoseStreak ? WinStreak : LoseStreak);
        money += GetInterestGold();

        AddMoney(money);
    }

    private int GetStreakGold(int streak)
    {
        if (streak >= 5)
        {
            return 3;
        }

        switch (streak)
        {
            case 2:
                return 1;
            case 3:
                return 1;
            case 4:
                return 2;
            default:
                return 0;
        }
    }

    private int GetInterestGold()
    {
        int interest = Mathf.FloorToInt(Money / 10.0f);

        return Mathf.Min(interest, MaxInterest);
    }
}
