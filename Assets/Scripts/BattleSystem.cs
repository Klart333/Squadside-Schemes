using Cysharp.Threading.Tasks;
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

    public async void StartBattle(IEnumerable<UnitNetworkData> unitsNetworkData, bool PVEEnemies, ulong opponentClientId)
    {
        // Spawn the enemy units locally

        List<Unit> unitsToHide = PlayerHandler.BoardSystem.UnitsOnBoard;
        List<Unit> alliedUnits = new List<Unit>();
        List<Unit> enemyUnits = new List<Unit>();

        // Hide own units
        for (int i = 0; i < unitsToHide.Count; i++)
        {
            GameManager.Instance.ToggleUnitVisibilityServerRPC(unitsToHide[i].NetworkObjectId, false);

            Unit unit = PlayerHandler.BoardSystem.SpawnUnitLocal(PlayerHandler.BoardSystem.UnitsOnBoardNetwork[i], false);
            await UniTask.WaitUntil(() => unit.IsInitialized);

            if (PlayerHandler.BoardSystem.UnitsOnBoardNetwork[i].ItemIndex0 != -1) unit.ApplyItem(GameManager.Instance.ItemDataUtility.Get(PlayerHandler.BoardSystem.UnitsOnBoardNetwork[i].ItemIndex0));
            if (PlayerHandler.BoardSystem.UnitsOnBoardNetwork[i].ItemIndex1 != -1) unit.ApplyItem(GameManager.Instance.ItemDataUtility.Get(PlayerHandler.BoardSystem.UnitsOnBoardNetwork[i].ItemIndex1));
            if (PlayerHandler.BoardSystem.UnitsOnBoardNetwork[i].ItemIndex2 != -1) unit.ApplyItem(GameManager.Instance.ItemDataUtility.Get(PlayerHandler.BoardSystem.UnitsOnBoardNetwork[i].ItemIndex2));

            alliedUnits.Add(unit);
        }

        foreach (var unitNetworkData in unitsNetworkData)
        {
            if (unitNetworkData.UnitDataIndex == -1)
            {
                Debug.LogError("Unit is null");
                continue;
            }

            Unit unit = PlayerHandler.BoardSystem.SpawnUnitLocal(unitNetworkData, true);
            await UniTask.WaitUntil(() => unit.IsInitialized);

            if (unitNetworkData.ItemIndex0 != -1) unit.ApplyItem(GameManager.Instance.ItemDataUtility.Get(unitNetworkData.ItemIndex0));
            if (unitNetworkData.ItemIndex1 != -1) unit.ApplyItem(GameManager.Instance.ItemDataUtility.Get(unitNetworkData.ItemIndex1));
            if (unitNetworkData.ItemIndex2 != -1) unit.ApplyItem(GameManager.Instance.ItemDataUtility.Get(unitNetworkData.ItemIndex2));

            unit.IsEnemyUnit = true;
            enemyUnits.Add(unit);
        }

        battle = new Battle
        {
            HiddenGuys = unitsToHide,
            AlliedUnits = alliedUnits,
            EnemyUnits = enemyUnits,
            ActiveBoardSystem = this.PlayerHandler.BoardSystem,

            OwnerClientID = PlayerHandler.OwnerClientId,
            IsAlliesFirst = PlayerHandler.OwnerClientId < opponentClientId,
            EnemyHasCandy = PVEEnemies
        };

        enemyUnits.ForEach((unit) => { unit.UpdateCachedTraits(enemyUnits); unit.OnPlacedOnBoard(); });
        alliedUnits.ForEach((unit) => { unit.UpdateCachedTraits(alliedUnits); unit.OnPlacedOnBoard(); });

        await UniTask.Delay(1000);

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
            battle.AlliedUnits[i].OnDeath -= battle.Ally_OnDeath;
            battle.AlliedUnits[i].LocalDeath();
        }

        for (int i = 0; i < battle.EnemyUnits.Count; i++)
        {
            battle.EnemyUnits[i].OnDeath -= battle.Enemy_OnDeath;
            battle.EnemyUnits[i].LocalDeath();
        }

        for (int i = 0; i < battle.HiddenGuys.Count; i++)
        {
            GameManager.Instance.ToggleUnitVisibilityServerRPC(battle.HiddenGuys[i].NetworkObjectId, true);

            battle.HiddenGuys[i].CurrentTile.CurrentUnit = battle.HiddenGuys[i];
        }

        battle = null;
    }

    public bool Overtime()
    {
        if (battle == null)
        {
            return false;
        }

        battle.Overtime();

        return true;
    }
}
