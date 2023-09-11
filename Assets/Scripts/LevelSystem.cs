using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem : SerializedMonoBehaviour
{
    public event Action OnXPChanged;
    public event Action OnLevelUp;

    [TitleGroup("Level Odds")]
    [ShowInInspector]
    [OdinSerialize]
    public List<float[]> LevelOdds = new List<float[]>();

    [TitleGroup("XP Thresholds")]
    public int[] xpThresholds;

    private int currentXP;

    public int CurrentLevel { get; private set; } = 1;
    public PlayerHandler PlayerHandler { get; set; }

    public int CurrentXP => currentXP;
    public float[] CurrentOdds => LevelOdds[CurrentLevel - 1];

    public void AddXP(int amount)
    {
        currentXP += amount;

        while (CurrentLevel <= xpThresholds.Length && currentXP >= xpThresholds[CurrentLevel - 1])
        {
            currentXP -= xpThresholds[CurrentLevel - 1];
            LevelUp();
        }

        OnXPChanged?.Invoke();
    }

    public void StartRound()
    {
        AddXP(2);
    }

    private void LevelUp()
    {
        CurrentLevel++;
        OnLevelUp?.Invoke();
    }
}
