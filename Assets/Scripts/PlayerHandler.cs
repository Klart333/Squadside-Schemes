using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Linq;
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

    [TitleGroup("UI")]
    [SerializeField]
    private PlayerUI playerUIPrefab;

    public LevelSystem LevelSystem { get; private set; }
    public BoardSystem BoardSystem { get; private set; }
    public InteractSystem InteractSystem { get; private set; }
    public MoneySystem MoneySystem { get; private set; }
    public BattleSystem BattleSystem { get; private set; }
    public HealthSystem HealthSystem { get; private set; }
    public PlayerUI PlayerUI { get; private set; }

    public NetworkVariable<int> Playerhealth = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);

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

        PlayerUI = Instantiate(playerUIPrefab);
        PlayerUI.PlayerHandler = this;

        GameManager.Instance.StartBattle += StartBattle;

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

        GameManager.Instance.StartBattle -= StartBattle;
    }

    private void HealthSystem_OnHealthChanged()
    {
        Playerhealth.Value = HealthSystem.CurrentHealth;
    }

    #region Spawn Board

    [ServerRpc]
    private void SpawnBoardSystemServerRPC(ServerRpcParams serverRpcParams)
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

    private void StartBattle(BoardSystem board1, BoardSystem board2, int activeIndex)
    {
        //Debug.Log("Starting battle at: " + OwnerClientId);

        BoardSystem enemyBoard = board1 == this.BoardSystem ? board2 : board1;

        this.BattleSystem.StartBattle(enemyBoard.UnitsOnBoardNetwork, enemyBoard.OwnerClientId);
    }


    [ClientRpc]
    public void WinBattleClientRPC(ClientRpcParams param)
    {
        if (!IsOwner)
        {
            return;
        }

        //Debug.Log("Won Battle! " + OwnerClientId);

        this.MoneySystem.LoseStreak = 0;
        this.MoneySystem.WinStreak += 1;

        MoneySystem.AddMoney(1);
    }

    [ClientRpc]
    public void LoseBattleClientRPC(int damage, ClientRpcParams param)
    {
        if (!IsOwner)
        {
            return;
        }

        this.MoneySystem.LoseStreak += 1;
        this.MoneySystem.WinStreak = 0;

        HealthSystem.LoseHealth(damage);

        //Debug.Log("Lost Battle! " + OwnerClientId);

        //MoneySystem.AddMoney(-1000);
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
    public void UpdateUIHealthClientRPC(int[] playerHealths, ClientRpcParams param)
    {
        if (!IsOwner)
        {
            return;
        }

        PlayerUI.UpdateAllPlayerHealth(playerHealths);
    }


    [ClientRpc]
    public void SetupUIHealthClientRPC(int playerCount, ClientRpcParams param)
    {
        if (!IsOwner)
        {
            return;
        }

        PlayerUI.SetupPlayerHealths(playerCount);
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

}
