using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
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

        NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerHandlers = FindObjectsOfType<PlayerHandler>();

        if (playerHandlers.Length == 2)
        {
            if (IsOwner)
            {
                StartGameServerRPC();
            }
            else
            {
                Debug.Log("Not starting because we're not the owner");
            }
        }
    }

    [ServerRpc]
    private void StartGameServerRPC()
    {
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
        if (!IsServer)
        {
            return;
        }

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
        if (!IsServer)
        {
            return;
        }
        Debug.Log("Starting New Round");

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
        for (int i = 0; i < playerHandlers.Length; i++)
        {
            playerHandlers[i].BoardSystem.HasUpdatedUnitsOnBoard.Value = false;
            playerHandlers[i].BoardSystem.UpdateUnitsOnBoardClientRPC();
        }
    }

    private async void StartBattles()
    {
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
            StartBattleClientRPC(i, i + 1, clientParams);
        }

        EvaluateBattleResults();
    }

    private async void EvaluateBattleResults()
    {
        for (int i = 0; i < battlePairings.Length; i++)
        {
            await UniTask.WaitUntil(() => battlePairings[i].Player1Reported && battlePairings[i].Player2Reported);

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

        await Task.Delay(2000);

        for (int i = 0; i < playerHandlers.Length; i++)
        {
            playerHandlers[i].EndBattleClientRPC();
        }

        // Start new round
        StartNewRound();
    }

    [ClientRpc]
    private void StartBattleClientRPC(int index1, int index2, ClientRpcParams clientParams)
    {
        int activeIndex = 0;

        Debug.Log("Start Battle, Length: " + playerHandlers.Length);

        StartBattle?.Invoke(playerHandlers[index1].BoardSystem, playerHandlers[index2].BoardSystem, activeIndex);

    }

    [ServerRpc(RequireOwnership = false)]
    public void ReportBattleServerRPC(bool wonBattle, int unitCount, ServerRpcParams param)
    {
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
        if (NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(objectId))
        {
            NetworkManager.SpawnManager.SpawnedObjects[objectId].Despawn(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnServerRPC(ulong objectId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(objectId))
        {
            NetworkManager.SpawnManager.SpawnedObjects[objectId].Despawn(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleUnitVisibilityServerRPC(ulong unitID, bool value)
    {
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
