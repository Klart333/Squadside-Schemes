﻿using System.Collections.Generic;
using UnityEngine;

public class UIPlayerHealthHandler : MonoBehaviour
{
    [SerializeField]
    private PlayerUI playerUI;

    [SerializeField]
    private UIPlayerHealth playerHealthPrefab;

    private UIPlayerHealth[] playerHealths;
    private List<UIPlayerHealth> sortedPlayerHealths = new List<UIPlayerHealth>();

    public int PlayerCount => playerHealths.Length;

    public void Setup(int playerAmount, List<User> steamUsers)
    {
        playerHealths = new UIPlayerHealth[playerAmount];
        for (int i = 0; i < playerAmount; i++)
        {
            playerHealths[i] = (Instantiate(playerHealthPrefab, transform));

            playerHealths[i].Setup(steamUsers[i], playerUI.PlayerHandler.HealthSystem.StartingHealth);
        }

        sortedPlayerHealths = new List<UIPlayerHealth>(playerHealths);
    }

    public void UpdateHealth(int index, int health)
    {
        playerHealths[index].UpdateHealth(health);

        sortedPlayerHealths.Sort((x, y) => x.CurrentHealth.CompareTo(y.CurrentHealth));

        for (int i = 0; i < sortedPlayerHealths.Count; i++)
        {
            int siblingIndex = sortedPlayerHealths.Count - 1 - i;
            sortedPlayerHealths[i].transform.SetSiblingIndex(siblingIndex);
        }
    }
}
