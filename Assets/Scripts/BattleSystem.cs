using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    public PlayerHandler PlayerHandler { get; set; }

    private Battle battle;

    public bool IsInBattle => battle != null;

    private void Update()
    {
        if (battle != null)
        {
            battle.Update();
        }
    }

    public async void StartBattle(NetworkList<UnitNetworkData> unitsNetworkData, ulong opponentClientId)
    {
        // Spawn the enemy units locally

        List<Unit> unitsToHide = PlayerHandler.BoardSystem.UnitsOnBoard;
        List<Unit> alliedUnits = new List<Unit>();
        List<Unit> enemyUnits = new List<Unit>();

        // Hide own units
        for (int i = 0; i < unitsToHide.Count; i++)
        {
            GameManager.Instance.ToggleUnitVisibilityServerRPC(unitsToHide[i].NetworkObjectId, false);

            alliedUnits.Add(PlayerHandler.BoardSystem.SpawnUnitLocal(PlayerHandler.BoardSystem.UnitsOnBoardNetwork[i], false));
        }

        foreach (var item in unitsNetworkData)
        {
            if (item.UnitDataIndex == -1)
            {
                Debug.LogError("Unit is null");
                continue;
            }
            Unit unit = PlayerHandler.BoardSystem.SpawnUnitLocal(item, true);
            enemyUnits.Add(unit);
        }

        await Task.Delay(1000);

        battle = new Battle
        {
            HiddenGuys = unitsToHide,
            AlliedUnits = alliedUnits,
            EnemyUnits = enemyUnits,
            ActiveBoardSystem = this.PlayerHandler.BoardSystem,

            OwnerClientID = PlayerHandler.OwnerClientId,
            IsAlliesFirst = PlayerHandler.OwnerClientId < opponentClientId
        };

        battle.StartBattle();
    }

    public void EndBattle()
    {
        Debug.Log("Ending battle");

        if (battle == null)
        {
            Debug.Log("Nvm");

            return;
        }

        for (int i = 0; i < battle.AlliedUnits.Count; i++)
        {
            Destroy(battle.AlliedUnits[i].gameObject);
        }

        for (int i = 0; i < battle.EnemyUnits.Count; i++)
        {
            Destroy(battle.EnemyUnits[i].gameObject);
        }

        for (int i = 0; i < battle.HiddenGuys.Count; i++)
        {
            GameManager.Instance.ToggleUnitVisibilityServerRPC(battle.HiddenGuys[i].NetworkObjectId, true);

            battle.HiddenGuys[i].CurrentTile.CurrentUnit = battle.HiddenGuys[i];
        }

        battle = null;
    }
}
