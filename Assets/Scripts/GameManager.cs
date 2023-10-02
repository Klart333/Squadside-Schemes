using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public event Action<BoardSystem, BoardSystem, int> StartBattle;

    public const float RoundLength = 20;

    public UnitDataUtility UnitDataUtility;
    public TraitUtility TraitUtility;

    private PlayerHandler[] playerHandlers;

    private ServerBattleData[] battlePairings;

    private float roundTimer = 0;
    private bool inPlanningPhase = false;

    public static GameManager Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

#if DEDICATED_SERVER
        StartGame();
#endif

    }

    private async void StartGame()
    {
        Debug.Log("StartGame");

        await UniTask.Delay(TimeSpan.FromSeconds(1)); // To allow the gamemanager to spawn, the startgame message was previously faster than the spawn message

        playerHandlers = FindObjectsOfType<PlayerHandler>();

        List<PlayerHandler> sorted = playerHandlers.ToList();
        sorted.Sort((x, y) => x.OwnerClientId.CompareTo(y.OwnerClientId));

        playerHandlers = sorted.ToArray();

        for (int i = 0; i < playerHandlers.Length; i++)
        {
            playerHandlers[i].Playerhealth.OnValueChanged += UpdateUIHealth;

            var param = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { playerHandlers[i].OwnerClientId } } };
            playerHandlers[i].SetupUIHealthClientRPC(playerHandlers.Length, param);
        }

        StartNewRound();
    }

    public void UpdateUIHealth(int previousValue, int newValue)
    {
        Debug.Log("UpdateUIHealth");

        int[] healts = new int[playerHandlers.Length];
        for (int i = 0; i < playerHandlers.Length; i++)
        {
            healts[i] = playerHandlers[i].Playerhealth.Value;
        }

        for (int i = 0; i < playerHandlers.Length; i++)
        {
            if ((int)playerHandlers[i].OwnerClientId != i)
            {
                Debug.LogError("Oh noes!");
            }

            var param = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { playerHandlers[i].OwnerClientId } } };
            playerHandlers[i].UpdateUIHealthClientRPC(healts, param);
        }
    }

    private void Update()
    {
#if !DEDICATED_SERVER
        return;
#endif

        if (inPlanningPhase)
        {
            roundTimer += Time.deltaTime;

            if (roundTimer >= RoundLength)
            {
                for (int i = 0; i < playerHandlers.Length; i++)
                {
                    playerHandlers[i].EndPlanningPhaseClientRPC();
                }

                Debug.Log("Server is starting Battles!");
                inPlanningPhase = false;
                roundTimer = 0;

                StartBattles();
            }
        }
    }

    private void StartNewRound()
    {
        Debug.Log("StartNewRound");

        for (int i = 0; i < playerHandlers.Length; i++)
        {
            playerHandlers[i].StartPlanningPhaseClientRPC();
        }

        inPlanningPhase = true;
        roundTimer = 0;
    }

#region Battle
    private void UpdateClientsBoard()
    {
        Debug.Log("UpdateClientsBoard");

        for (int i = 0; i < playerHandlers.Length; i++)
        {
            playerHandlers[i].BoardSystem.HasUpdatedUnitsOnBoard.Value = false;
            playerHandlers[i].BoardSystem.UpdateUnitsOnBoardClientRPC();
        }
    }

    private async void StartBattles()
    {
        Debug.Log("StartBattles");

        UpdateClientsBoard();

        await Task.Delay(100);

        for (int i = 0; i < playerHandlers.Length; i++)
        {
            await UniTask.WaitUntil(() => playerHandlers[i].BoardSystem.HasUpdatedUnitsOnBoard.Value);
        }

        battlePairings = new ServerBattleData[Mathf.FloorToInt(playerHandlers.Length / 2.0f)];
        int battleIndex = 0;
        for (int i = 0; i < playerHandlers.Length; i += 2)
        {
            if (i + 1 >= playerHandlers.Length)
            {
                Debug.Log("Uuh i cant handle ghost boards, idk how");
                continue;
            }

            ulong player1ID = playerHandlers[i].OwnerClientId;
            ulong player2ID = playerHandlers[i + 1].OwnerClientId;
            battlePairings[battleIndex++] = new ServerBattleData() { Player1Id = player1ID, Player2Id = player2ID };

            ClientRpcParams clientParams = new ClientRpcParams() { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { player1ID, player2ID } } };
            Debug.Log("Starting Battles on Clients from Server");

            for (int g = 0; g < playerHandlers.Length; g++)
            {
                playerHandlers[g].StartBattleClientRPC(player1ID, player2ID, clientParams);
            }
        }

        EvaluateBattleResults();
    }

    private async void EvaluateBattleResults()
    {
        Debug.Log("EvaluateBattleResults");

        int amountDone = 0;
        while (amountDone < battlePairings.Length)
        {
            amountDone = 0;
            for (int i = 0; i < battlePairings.Length; i++)
            {
                if (!battlePairings[i].Player1Reported || !battlePairings[i].Player2Reported)
                {
                    continue;
                }

                amountDone++;
                ulong winner = battlePairings[i].EvaluateWinner();
                ulong loser = battlePairings[i].Player1Id == winner ? battlePairings[i].Player2Id : battlePairings[i].Player1Id;
                int unitCount = battlePairings[i].Player1Id == winner ? battlePairings[i].Player1UnitCount : battlePairings[i].Player2UnitCount;
                int damage = 4 + unitCount * 2;

                Debug.Log("The winner is " + winner + ", and the loser is " + loser);

                // Take damage, and gain one gold
                ClientRpcParams winnerParam = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { winner } } };
                ClientRpcParams loserParam = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { loser } } };

                for (int g = 0; g < playerHandlers.Length; g++)
                {
                    if (playerHandlers[g].OwnerClientId == winner)
                    {
                        playerHandlers[g].WinBattleClientRPC(winnerParam);
                    }
                    else if (playerHandlers[g].OwnerClientId == loser)
                    {
                        playerHandlers[g].LoseBattleClientRPC(damage, loserParam);
                    }
                }
            }

            if (amountDone < battlePairings.Length)
            {
                Debug.Log("Server Waiting for client battle report");
                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
        }

        await Task.Delay(2000);

        for (int i = 0; i < playerHandlers.Length; i++)
        {
            playerHandlers[i].EndBattleClientRPC();
        }

        // Start new round
        StartNewRound();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReportBattleServerRPC(bool wonBattle, int unitCount, ServerRpcParams param)
    {
        Debug.Log("ReportBattleServerRPC");

        for (int i = 0; i < battlePairings.Length; i++)
        {
            if (battlePairings[i].Player1Id == param.Receive.SenderClientId)
            {
                battlePairings[i].Player1Won = wonBattle;
                battlePairings[i].Player1Reported = true;
                battlePairings[i].Player1UnitCount = unitCount;

                break;
            }

            if (battlePairings[i].Player2Id == param.Receive.SenderClientId)
            {
                battlePairings[i].Player2Won = wonBattle;
                battlePairings[i].Player2Reported = true;
                battlePairings[i].Player2UnitCount = unitCount;

                break;
            }
        }
    }
#endregion

#region Show/Hide

    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRPC(ulong objectId)
    {
        Debug.Log("DestroyServerRPC");
        if (NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(objectId))
        {
            NetworkManager.SpawnManager.SpawnedObjects[objectId].Despawn(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnServerRPC(ulong objectId)
    {
        Debug.Log("DespawnServerRPC");

        if (NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(objectId))
        {
            NetworkManager.SpawnManager.SpawnedObjects[objectId].Despawn(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleUnitVisibilityServerRPC(ulong unitID, bool value)
    {
        Debug.Log("ToggleUnitVisibilityServerRPC");

        for (int i = 0; i < playerHandlers.Length; i++)
        {
            var param = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { playerHandlers[i].OwnerClientId } } };

            playerHandlers[i].ToggleUnitVisibilityClientRPC(unitID, value, param);
        }
    }
#endregion
}

public struct ServerBattleData
{
    public ulong Player1Id;
    public ulong Player2Id;

    public bool Player1Won;
    public bool Player2Won;

    public bool Player1Reported;
    public bool Player2Reported;

    public int Player1UnitCount;
    public int Player2UnitCount;

    public ulong EvaluateWinner()
    {
        if (Player1Won && !Player2Won)
        {
            return Player1Id;
        }
        else if (!Player1Won && Player2Won)
        {
            return Player2Id;
        }
        else
        {
            Debug.Log("Conflicting winner, oh noes!");

            if (Player1UnitCount > Player2UnitCount) // I guess just return whoever won more
            {
                return Player1Id;
            }
            else
            {
                return Player2Id;
            }
        }
    }
}
