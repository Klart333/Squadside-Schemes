using System;
using System.Collections.Generic;
using Unity.Netcode;

public class Battle
{
    public List<Unit> AlliedUnits = new List<Unit>();
    public List<Unit> EnemyUnits = new List<Unit>();
    public List<Unit> HiddenGuys = new List<Unit>();

    public BoardSystem ActiveBoardSystem;
    public bool IsAlliesFirst = false;

    public Tile[,] Board => ActiveBoardSystem.Tiles;

    public ulong OwnerClientID;
    public ulong OpponentClientID;

    public bool EnemyHasCandy;

    private bool reported = false;

    public void StartBattle()
    {
        for (int i = 0; i < AlliedUnits.Count; i++)
        {
            AlliedUnits[i].StartCombat();
            AlliedUnits[i].OnDeath += Ally_OnDeath;
        }

        for (int i = 0; i < EnemyUnits.Count; i++)
        {
            EnemyUnits[i].StartCombat();
            EnemyUnits[i].OnDeath += Enemy_OnDeath;
        }
    }

    public void Enemy_OnDeath(Unit unit)
    {
        unit.OnDeath -= Enemy_OnDeath;
        EnemyUnits.Remove(unit);

        if (EnemyHasCandy)
        {
            ActiveBoardSystem.PlayerHandler.LootSystem.MaybeSpawnLoot(unit.CurrentTile.WorldPosition);
        }
    }

    public void Ally_OnDeath(Unit unit)
    {
        unit.OnDeath -= Ally_OnDeath;
        AlliedUnits.Remove(unit);
    }

    public void Update()
    {
        if (reported)
        {
            return;
        }

        if (EnemyUnits.Count == 0)
        {
            Win();
            reported = true;

            return;
        }

        if (AlliedUnits.Count == 0)
        {
            Lose();
            reported = true;

            return;
        }

        if (IsAlliesFirst)
        {
            UpdateUnits(AlliedUnits);
            UpdateUnits(EnemyUnits);
        }
        else
        {
            UpdateUnits(EnemyUnits);
            UpdateUnits(AlliedUnits);
        }
    }

    private void UpdateUnits(List<Unit> units)
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (AlliedUnits.Count == 0 || EnemyUnits.Count == 0)
            {
                return;
            }

            units[i]?.UpdateBattle(this);
        }
    }

    private void Win()
    {
        ServerRpcParams param = new ServerRpcParams() { Receive = new ServerRpcReceiveParams() { SenderClientId = OwnerClientID } };
        GameManager.Instance.ReportBattleServerRPC(true, AlliedUnits.Count, param);
    }

    private void Lose()
    {
        ServerRpcParams param = new ServerRpcParams() { Receive = new ServerRpcReceiveParams() { SenderClientId = OwnerClientID } };
        GameManager.Instance.ReportBattleServerRPC(false, 0, param);
    }

    public List<Unit> GetEnemyUnits(Unit unit)
    {
        if (EnemyUnits.Contains(unit))
        {
            return AlliedUnits;
        }

        return EnemyUnits;
    }

    public void Overtime()
    {
        for (int i = 0; i < AlliedUnits.Count; i++)
        {
            AlliedUnits[i].BattleController.ActivateOvertime();
        }

        for (int i = 0; i < EnemyUnits.Count; i++)
        {
            EnemyUnits[i].BattleController.ActivateOvertime();
        }
    }
}
