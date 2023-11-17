using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MoneySystem : MonoBehaviour
{
    public event Action OnAmountChanged;

    [SerializeField]
    private Trait Hustler;

    [SerializeField]
    private Coin coinPrefab;

    public PlayerHandler PlayerHandler { get; set; }

    public int Money { get; private set; } = 20;
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

        if (WinStreak > LoseStreak)
        {
            money += GetStreakGold(WinStreak);
        }
        else
        {
            int mult = 1;
            Dictionary<Trait, int> trats = GameManager.Instance.TraitUtility.GetTraits(PlayerHandler.BoardSystem.UnitsOnBoard);
            if (trats != null && trats.ContainsKey(Hustler))
            {
                mult = 2;
            }

            money += GetStreakGold(LoseStreak) * mult;
        }

        money += GetInterestGold();

        AddMoney(money);

        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.GoldSFX);
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

    public async void SpawnMoney(int moneyAmount, Vector3 position)
    {
        for (int i = 0; i < moneyAmount; i++)
        {
            Vector3 rand = UnityEngine.Random.insideUnitSphere * 2;
            Vector3 randomPos = position + rand;
            randomPos.y = 0.1f;

            Coin coin = coinPrefab.GetAtPosAndRot<Coin>(position, Quaternion.identity);
            coin.MoneySystem = this;
            coin.AnimateToPosition(randomPos);

            await UniTask.Delay(100);
        }
    }
}
