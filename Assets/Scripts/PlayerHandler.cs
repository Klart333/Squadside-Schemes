using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Steamworks;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHandler : NetworkBehaviour
{
    [TitleGroup("Systems")]
    [SerializeField]
    private LevelSystem levelSystemPrefab;

    [SerializeField]
    private BoardSystem boardSystemPrefab;

    [SerializeField]
    private InteractSystem interactSystemPrefab;

    [SerializeField]
    private MoneySystem moneySystemPrefab;

    [SerializeField]
    private BattleSystem battleSystemPrefab;

    [SerializeField]
    private HealthSystem healthSystemPrefab;

    [SerializeField]
    private LootSystem lootSystemPrefab;

    [TitleGroup("UI")]
    [SerializeField]
    private PlayerUI playerUIPrefab;

    public LevelSystem LevelSystem { get; private set; }
    public BoardSystem BoardSystem { get; private set; }
    public InteractSystem InteractSystem { get; private set; }
    public MoneySystem MoneySystem { get; private set; }
    public BattleSystem BattleSystem { get; private set; }
    public HealthSystem HealthSystem { get; private set; }
    public LootSystem LootSystem { get; private set; }
    public PlayerUI PlayerUI { get; private set; }

    public NetworkVariable<int> Playerhealth = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> PlayerElo = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<ulong> PlayerSteamID = new NetworkVariable<ulong>(writePerm: NetworkVariableWritePermission.Owner);

    public bool InteractionRestricted => PlayerUI.TimerDisplay.Percent > 0.95f;

    private void Start()
    {
        Vector3 offset = Vector3.left * 10 * OwnerClientId;
        transform.position = offset;

        if (!IsOwner)
        {
            BoardSystem[] boardSystems = FindObjectsOfType<BoardSystem>();
            for (int i = 0; i < boardSystems.Length; i++)
            {
                if (boardSystems[i].OwnerClientId == this.OwnerClientId)
                {
                    this.BoardSystem = boardSystems[i];
                    break;
                }
            }

            return;
        }

        PlayerSteamID.Value = SteamUser.GetSteamID().m_SteamID;
        PlayerElo.Value = PlayerRankManager.Instance.GetElo();

        ServerRpcParams serverRpcParams = new ServerRpcParams() { Receive = new ServerRpcReceiveParams() { SenderClientId = this.OwnerClientId } };
        SpawnBoardSystemServerRPC(serverRpcParams);

        Camera.main.transform.position += offset;

        LevelSystem = Instantiate(levelSystemPrefab, transform);
        LevelSystem.PlayerHandler = this;

        InteractSystem = Instantiate(interactSystemPrefab, transform);
        InteractSystem.PlayerHandler = this;

        MoneySystem = Instantiate(moneySystemPrefab, transform);
        MoneySystem.PlayerHandler = this;

        BattleSystem = Instantiate(battleSystemPrefab, transform);
        BattleSystem.PlayerHandler = this;

        HealthSystem = Instantiate(healthSystemPrefab, transform);
        HealthSystem.PlayerHandler = this;

        LootSystem = Instantiate(lootSystemPrefab, transform);
        LootSystem.PlayerHandler = this;

        PlayerUI = Instantiate(playerUIPrefab);
        PlayerUI.PlayerHandler = this;

        Playerhealth.Value = HealthSystem.StartingHealth;
        HealthSystem.OnHealthChanged += HealthSystem_OnHealthChanged;
    }

    private void OnDisable()
    {
        if (!IsOwner)
        {
            return;
        }

        if (HealthSystem != null)
        {
            HealthSystem.OnHealthChanged -= HealthSystem_OnHealthChanged;
        }
    }

    private void HealthSystem_OnHealthChanged()
    {
        Playerhealth.Value = HealthSystem.CurrentHealth;
    }

    #region Spawn Board

    [ServerRpc]
    private void SpawnBoardSystemServerRPC(ServerRpcParams serverRpcParams) // Think a bit about this, boardsystem isn't being set for other clients, maybe remove the TargetClientIds? 
    {
        BoardSystem = Instantiate(boardSystemPrefab, transform);

        NetworkObject networkObject = BoardSystem.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(serverRpcParams.Receive.SenderClientId, true);

        ClientRpcParams clientRpcParams = new ClientRpcParams() { Send = new ClientRpcSendParams { TargetClientIds = new[] { serverRpcParams.Receive.SenderClientId } } };
        SetBoardSystemClientRPC(networkObject.NetworkObjectId, clientRpcParams);
    }

    [ClientRpc]
    private void SetBoardSystemClientRPC(ulong boardNetworkID, ClientRpcParams clientRpcParams)
    {
        this.BoardSystem = NetworkManager.Singleton.SpawnManager.SpawnedObjects[boardNetworkID].GetComponent<BoardSystem>();
        BoardSystem.PlayerHandler = this;
    }
    #endregion

    #region Battle

    [ClientRpc]
    public void StartBattleClientRPC(ulong id1, ulong id2, ClientRpcParams clientParams)
    {
        if (!IsOwner)
        {
            return;
        }

        Debug.Log("StartBattleClientRPC");

        BoardSystem[] boardSystems = FindObjectsOfType<BoardSystem>();

        BoardSystem board1 = null;
        BoardSystem board2 = null;

        for (int i = 0; i < boardSystems.Length; i++)
        {
            if (boardSystems[i].OwnerClientId == id1)
            {
                board1 = boardSystems[i];
            }

            if (boardSystems[i].OwnerClientId == id2)
            {
                board2 = boardSystems[i];
            }
        }

        if (!board1 || !board2)
        {
            Debug.LogError("Could not find enemy playerhandler or board: " + board1 + ", " + board2);
            return;
        }

        BoardSystem enemyBoard = board1 == this.BoardSystem ? board2 : board1;

        List<UnitNetworkData> networkList = new List<UnitNetworkData>(enemyBoard.UnitsOnBoardNetwork.Count);
        for (int i = 0; i < enemyBoard.UnitsOnBoardNetwork.Count; i++)
        {
            networkList.Add(enemyBoard.UnitsOnBoardNetwork[i]);
        }

        this.BattleSystem.StartBattle(networkList, false, enemyBoard.OwnerClientId);
    }

    [ClientRpc]
    public void StartPVEBattleClientRPC(int mobUnitsIndex, ClientRpcParams clientParams)
    {
        if (!IsOwner)
        {
            return;
        }

        Debug.Log("StartPVEBattleClientRPC");

        this.BattleSystem.StartBattle(GameManager.Instance.PVEData.GetMobData(mobUnitsIndex), true, 1000); // 1000 to always go first
    }


    [ClientRpc]
    public void WinBattleClientRPC(bool isPVE, ClientRpcParams param)
    {
        if (!IsOwner)
        {
            return;
        }

        Debug.Log("Won Battle! " + OwnerClientId);

        if (!isPVE)
        {
            this.MoneySystem.LoseStreak = 0;
            this.MoneySystem.WinStreak += 1;
        }

        MoneySystem.AddMoney(1);
    }

    [ClientRpc]
    public void LoseBattleClientRPC(int damage, bool isPVE, ClientRpcParams param)
    {
        if (!IsOwner)
        {
            return;
        }

        if (!isPVE)
        {
            this.MoneySystem.LoseStreak += 1;
            this.MoneySystem.WinStreak = 0;
        }

        HealthSystem.LoseHealth(damage);

        Debug.Log("Lost Battle! " + OwnerClientId);
    }

    [ClientRpc]
    public void EndBattleClientRPC()
    {
        if (!IsOwner)
        {
            return;
        }

        BattleSystem.EndBattle();
    }
    #endregion

    #region Planning

    [ClientRpc]
    public void StartPlanningPhaseClientRPC()
    {
        if (!IsOwner)
        {
            return;
        }

        StartPlanningPhaseWAITCAUSEIMSTUPIDANDLAZY();
    }

    private async void StartPlanningPhaseWAITCAUSEIMSTUPIDANDLAZY()
    {
        await UniTask.WaitUntil(() => GameManager.Instance != null);

        PlayerUI.StartRound();
        LevelSystem.StartRound();
        MoneySystem.StartRound();
    }

    [ClientRpc]
    public void EndPlanningPhaseClientRPC()
    {
        if (!IsOwner)
        {
            return;
        }

        PlayerUI.EndRoundTimer();
    }

    #endregion

    [ClientRpc]
    public void SetupUIHealthClientRPC(int playerCount, ulong[] steamIds, ClientRpcParams param)
    {
        if (!IsOwner)
        {
            return;
        }

        PlayerUI.SetupPlayerHealths(playerCount, steamIds);
    }

    [ClientRpc]
    public void ToggleUnitVisibilityClientRPC(ulong unitID, bool value, ClientRpcParams param)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(unitID))
        {
            NetworkObject networkUnit = NetworkManager.Singleton.SpawnManager.SpawnedObjects[unitID];
            if (networkUnit.TryGetComponent(out Unit unit))
            {
                unit.ToggleVisibility(value);
            }
        }
    }

    [ClientRpc]
    public void EndGameClientRPC(bool lost, float opponentElo)
    {
        if (!IsOwner)
        {
            return;
        }

        PlayerUI.EndGame(lost);

        if (lost)
        {
            PlayerRankManager.Instance.LoseGame(opponentElo);
        }
        else
        {
            PlayerRankManager.Instance.WinGame(opponentElo);
        }
    }

    [ClientRpc]
    public void StartOvertimeClientRPC()
    {
        if (!IsOwner)
        {
            return;
        }

        if (BattleSystem.Overtime())
        {
            PlayerUI.Overtime();
        }
    }
}
