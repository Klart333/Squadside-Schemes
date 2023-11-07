using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Netcode;
using Cysharp.Threading.Tasks;
using System;

public class BoardSystem : NetworkBehaviour
{
    public event Action<List<Unit>> OnBoardedUnitsChanged;
    public event Action<Unit> OnUnitPickup;
    public event Action<Unit> OnUnitPlace;

    public const float TileScale = 1f;
    public const int BoardX = 7;
    public const int BoardY = 8;
    public const int BenchSlots = 9;

    [TitleGroup("Create Board")]
    [SerializeField]
    [PropertyOrder(-99)]
    private GameObject tilePrefab;

    [PropertyOrder(-99)]
    [SerializeField]
    private GameObject benchTilePrefab;

    [Title("Board Prefab")]
    [SerializeField]
    private GameObject boardPrefab;

    [Title("Net")]
    [SerializeField]
    private Net boardNetPrefab;

    [SerializeField]
    private Net benchNetPrefab;

    [Title("Particles")]
    [SerializeField]
    private PooledMonoBehaviour placeParticle;

    public List<Unit> UnitsOnBoard
    {
        get
        {
            List<Unit> units = new List<Unit>();

            foreach (var tile in boardTiles)
            {
                if (tile.CurrentUnit && !tile.CurrentUnit.IsEnemyUnit)
                {
                    units.Add(tile.CurrentUnit);
                }
            }

            return units;
        }
    }

    public NetworkList<UnitNetworkData> UnitsOnBoardNetwork;

    public NetworkVariable<bool> HasUpdatedUnitsOnBoard = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Server);

    private List<Unit> Units = new List<Unit>();

    private Tile[,] boardTiles;
    private Tile[] benchTiles;

    private Net[,] boardNets;
    private Net[] benchNets;

    private Net highlightedNet;
    private Camera cam;

    private bool netVisible = false;

    public Tile[,] Tiles => boardTiles;

    public PlayerHandler PlayerHandler { get; set; }

    [Button]
    [PropertyOrder(-99)]
    public void SpawnTiles()
    {
        GameObject parent = new GameObject("Board");
        GameObject boardparent = new GameObject("Board");
        GameObject benchParent = new GameObject("Bench");

        boardparent.transform.SetParent(parent.transform);
        benchParent.transform.SetParent(parent.transform);

        for (int x = 0; x < BoardX; x++)
        {
            for (int y = 0; y < BoardY; y++)
            {
                float offset = y % 2 == 1 ? .5f : 0;
                Vector3 pos = new Vector3(x + offset, 0, y * 0.75f);

                GameObject tile = Instantiate(tilePrefab, boardparent.transform);

                tile.transform.localPosition = pos;
            }
        }

        for (int i = 0; i < BenchSlots; i++)
        {
            Vector3 pos = new Vector3(i * 0.8f, 0, -1.2f);

            GameObject benchTile = Instantiate(benchTilePrefab, benchParent.transform);

            benchTile.transform.localPosition = pos;
        }
    }

    private void Awake()
    {
        UnitsOnBoardNetwork = new NetworkList<UnitNetworkData>(writePerm: NetworkVariableWritePermission.Owner);
    }

    private void Start()
    {
        Instantiate(boardPrefab, transform);

        if (!IsOwner)
        {
            return;
        }

        boardTiles = new Tile[BoardX, BoardY];
        benchTiles = new Tile[BenchSlots];

        boardNets = new Net[BoardX, BoardY / 2];
        benchNets = new Net[BenchSlots];
        SetupBoard();
        SetupBench();

        cam = Camera.main;

        ToggleNet(false);
    }

    private void SetupBoard()
    {
        for (int x = 0; x < BoardX; x++)
        {
            for (int y = 0; y < BoardY; y++)
            {
                float offset = y % 2 == 1 ? .5f : 0;
                Vector3 pos = new Vector3(x + offset, 0, y * 0.75f);

                Tile tile = new Tile()
                {
                    Index = new Vector2Int(x, y),
                    WorldPosition = transform.position + pos
                };
                boardTiles[x, y] = tile;

                if (y < BoardY / 2)
                {
                    Net net = Instantiate(boardNetPrefab, transform);
                    net.transform.localPosition = pos + Vector3.up * 0.055f;
                    boardNets[x, y] = net;
                }
            }
        }
    }

    private void SetupBench()
    {
        for (int i = 0; i < BenchSlots; i++)
        {
            Vector3 pos = new Vector3(i * 0.8f, 0, -1.2f);

            Tile tile = new Tile()
            {
                Index = new Vector2Int(i, -1),
                WorldPosition = transform.position + pos
            };
            benchTiles[i] = tile;

            Net benchNet = Instantiate(benchNetPrefab, transform);
            benchNet.transform.localPosition = pos + Vector3.up * 0.1f;
            benchNets[i] = benchNet;
        }
    }

    private void Update()
    {
        if (netVisible)
        {
            HighlightHoveredNet();
        }
    }

    private void HighlightHoveredNet()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            Vector3 pos = hit.point;
            Tile tile = GetClosestTile(pos);
            if (tile == null)
            {
                if (highlightedNet != null)
                {
                    highlightedNet.ResetNet();
                    highlightedNet = null;
                }
                return;
            }

            Net net;
            if (tile.IsBench)
            {
                net = benchNets[tile.Index.x];
            }
            else
            {
                net = boardNets[tile.Index.x, tile.Index.y];
            }

            if (net != highlightedNet)
            {
                if (highlightedNet != null)
                {
                    highlightedNet.ResetNet();
                }

                highlightedNet = net;
                net.Highlight();
            }
        }
    }

    public void ToggleNet(bool value)
    {
        highlightedNet = null;
        netVisible = value;

        foreach (Net boardNet in boardNets)
        {
            boardNet.ResetNet();
            boardNet.gameObject.SetActive(value);
        }

        foreach (Net benchNet in benchNets)
        {
            benchNet.ResetNet();
            benchNet.gameObject.SetActive(value);
        }
    }

    public void PlacingUnit(Unit unit)
    {
        ToggleNet(true);

        OnUnitPickup?.Invoke(unit);
    }

    public void PlaceUnit(Unit unit)
    {
        ToggleNet(false);

        Tile tile = GetClosestTile(unit.transform.position);

        OnUnitPlace?.Invoke(unit);

        if (tile != null)
        {
            PlaceUnitOnTile(unit, tile);
        }
    }

    public bool PlaceUnitOnTile(Unit unit, Tile targetTile)
    {
        if (!unit) return false;

        List<Unit> units = UnitsOnBoard;

        if (!targetTile.IsBench && (PlayerHandler.InteractionRestricted || (targetTile.CurrentUnit == null && !units.Contains(unit) && units.Count >= PlayerHandler.LevelSystem.CurrentLevel))) // Check if board is full
        {
            unit.transform.position = unit.CurrentTile.WorldPosition;
            return false;
        }

        if (unit.CurrentTile != null)
        {
            unit.CurrentTile.CurrentUnit = null;
        }

        if (targetTile.CurrentUnit != null)
        {
            PlaceUnitOnTile(targetTile.CurrentUnit, unit.CurrentTile);
        }

        unit.transform.position = targetTile.WorldPosition;

        unit.CurrentTile = targetTile;
        targetTile.CurrentUnit = unit;

        if ((!unit.IsOnBoard && !targetTile.IsBench) || (unit.IsOnBoard && targetTile.IsBench)) // Basically dont call the event if we're just moving units on the board around
        {
            OnBoardedUnitsChanged?.Invoke(UnitsOnBoard);
        }

        if (targetTile.IsBench)
        {
            unit.OnPlacedOnBench(); // A little weird to be under the event, but the traits update on the event so its needed
        }
        else
        {
            unit.OnPlacedOnBoard();
        }

        return true;
    }

    public Tile GetClosestTile(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);

        if (position.z < -0.5f)
        {
            if (position.z < -2.0f)
            {
                return null; // Selling unit
            }

            return GetClosestBenchTile(position);
        }
        else
        {
            return GetClosestBoardTile(position);
        }
    }

    private Tile GetClosestBoardTile(Vector3 position, bool restricted = true)
    {
        int y = Mathf.RoundToInt(position.z / 0.75f);
        float offset = y % 2 == 1 ? .5f : 0;

        int x = Mathf.RoundToInt(position.x - offset);

        x = Mathf.Clamp(x, 0, BoardX - 1);

        int boardY = restricted ? BoardY / 2 : BoardY;
        y = Mathf.Clamp(y, 0, boardY - 1);

        return boardTiles[x, y];
    }

    private Tile GetClosestBenchTile(Vector3 position)
    {
        int index = Mathf.RoundToInt(position.x / 0.8f);

        index = Mathf.Clamp(index, 0, BenchSlots - 1);

        return benchTiles[index];
    }

    #region Spawn Unit

    public Unit SpawnUnitLocal(UnitNetworkData data, bool mirrored)
    {
        if (data.UnitDataIndex == -1)
        {
            return null;
        }

        int x = (BoardX - 1) - data.TileIndexX;
        int y = (BoardY - 1) - data.TileIndexY;
        if (!mirrored)
        {
            x = data.TileIndexX;
            y = data.TileIndexY;
        }

        Vector3 pos = Tiles[x, y].WorldPosition;

        UnitData unitData = GameManager.Instance.UnitDataUtility.Get(data.UnitDataIndex);
        Unit unit = Instantiate(unitData.UnitPrefab, pos, Quaternion.identity);

        for (int i = 0; i < data.StarLevel; i++)
        {
            unit.UpgradeStarLevel();
        }

        unit.CurrentTile = Tiles[x, y];

        unit.CurrentTile.CurrentUnit = unit;

        unit.PlayerHandler = this.PlayerHandler;

        return unit;
    }

    public bool SpawnUnit(UnitData unitData)
    {
        int index = -1;
        for (int i = 0; i < benchTiles.Length; i++)
        {
            if (benchTiles[i].CurrentUnit == null)
            {
                index = i;
                break;
            }
        }

        if (index == -1)
        {
            return false;
        }

        Vector3 pos = benchTiles[index].WorldPosition;
        placeParticle.GetAtPosAndRot<PooledMonoBehaviour>(pos + Vector3.up * 0.5f, Quaternion.identity);

        ServerRpcParams serverRpcParams = new ServerRpcParams() { Receive = new ServerRpcReceiveParams() { SenderClientId = this.OwnerClientId } };
        SpawnUnitServerRPC(unitData, pos, serverRpcParams);
        return true;
    }

    [ServerRpc]
    private void SpawnUnitServerRPC(UnitData unitData, Vector3 pos, ServerRpcParams serverRpcParams)
    {
        Unit unit = Instantiate(unitData.UnitPrefab, pos, Quaternion.identity);

        NetworkObject networkObject = unit.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(serverRpcParams.Receive.SenderClientId, true);

        ClientRpcParams clientRpcParams = new ClientRpcParams() { Send = new ClientRpcSendParams { TargetClientIds = new[] { serverRpcParams.Receive.SenderClientId } } };
        SetUnitClientRPC(networkObject.NetworkObjectId, clientRpcParams);
    }

    [ClientRpc]
    private void SetUnitClientRPC(ulong boardNetworkID, ClientRpcParams clientRpcParams)
    {
        Unit unit = NetworkManager.Singleton.SpawnManager.SpawnedObjects[boardNetworkID].GetComponent<Unit>();

        unit.PlayerHandler = PlayerHandler;
        //unit.transform.SetParent(transform);

        int index = -1;
        for (int i = 0; i < benchTiles.Length; i++)
        {
            if (benchTiles[i].CurrentUnit == null)
            {
                index = i;
                break;
            }
        }
        PlaceUnitOnTile(unit, benchTiles[index]);
        Units.Add(unit);

        CheckForUpgrade(unit);
    }

    #endregion

    private Queue<Unit> upgradeQueue = new Queue<Unit>();
    private bool upgrading = false;
    private async void CheckForUpgrade(Unit unit)
    {
        upgradeQueue.Enqueue(unit);
        if (upgrading)
        {
            return;
        }

        upgrading = true;

        while (upgradeQueue.TryDequeue(out Unit unitToUpgrade))
        {
            while (PlayerHandler.InteractionRestricted)
            {
                await UniTask.Yield();
            }

            int starlevel = unitToUpgrade.StarLevel;
            int ID = unitToUpgrade.UnitData.GetInstanceID();
            List<Unit> matchingUnits = new List<Unit>();

            for (int i = 0; i < Units.Count; i++)
            {
                if (matchingUnits.Count >= 3)
                {
                    break;
                }

                if (Units[i].UnitData.GetInstanceID() == ID && Units[i].StarLevel == starlevel)
                {
                    matchingUnits.Add(Units[i]);
                }
            }

            if (matchingUnits.Count >= 3) // Mat is Glad
            {
                Unit upgraded = await CombineUnits(matchingUnits);
                if (upgraded != null)
                {
                    await UniTask.WaitUntil(() => upgraded.StarLevel > starlevel);

                    upgradeQueue.Enqueue(upgraded);
                }
            }
        }

        upgrading = false;
    }

    private async UniTask<Unit> CombineUnits(List<Unit> units)
    {
        for (int i = 1; i < units.Count; i++)
        {
            if (units[i] == null)
            {
                return null;
            }

            units[i].IsInteractable = false;
        }

        for (int i = 1; i < units.Count; i++)
        {
            await UniTask.Delay(200);
            if (units[i] == null)
            {
                return null;
            }

            units[i].CurrentTile.CurrentUnit = null;
            this.Units.Remove(units[i]);

            GameManager.Instance.DestroyServerRPC(units[i].GetComponent<NetworkObject>().NetworkObjectId);
        }

        Unit starred = units[0];
        starred.IsInteractable = true;

        ParticleManager.Instance.LightParticle.GetAtPosAndRot<PooledMonoBehaviour>(starred.transform.position + Vector3.up, Quaternion.identity);

        starred.UpgradeStarLevelServerRPC();
        OnBoardedUnitsChanged?.Invoke(UnitsOnBoard);
        return starred;
    }

    [ClientRpc]
    public void UpdateUnitsOnBoardClientRPC()
    {
        if (!IsOwner)
        {
            return;
        }

        List<Unit> units = UnitsOnBoard;

        int max = Mathf.Max(UnitsOnBoardNetwork.Count, units.Count);
        for (int i = 0; i < max; i++)
        {
            if (i >= units.Count)
            {
                this.UnitsOnBoardNetwork[i] = new UnitNetworkData
                {
                    StarLevel = -1,
                    UnitDataIndex = -1,

                    TileIndexX = -1,
                    TileIndexY = -1,

                    ItemIndex0 = -1,
                    ItemIndex1 = -1,
                    ItemIndex2 = -1
                };

                continue;
            }

            UnitNetworkData unitNetworkData = new UnitNetworkData
            {
                StarLevel = units[i].StarLevel,
                UnitDataIndex = GameManager.Instance.UnitDataUtility.GetIndex(units[i].UnitData),

                TileIndexX = units[i].CurrentTile.Index.x,
                TileIndexY = units[i].CurrentTile.Index.y,

                ItemIndex0 = units[i].ItemSlots[0] == null ? -1 : GameManager.Instance.ItemDataUtility.GetIndex(units[i].ItemSlots[0]),
                ItemIndex1 = units[i].ItemSlots[1] == null ? -1 : GameManager.Instance.ItemDataUtility.GetIndex(units[i].ItemSlots[1]),
                ItemIndex2 = units[i].ItemSlots[2] == null ? -1 : GameManager.Instance.ItemDataUtility.GetIndex(units[i].ItemSlots[2])
            };

            if (i >= UnitsOnBoardNetwork.Count)
            {
                this.UnitsOnBoardNetwork.Add(unitNetworkData);
            }
            else
            {
                this.UnitsOnBoardNetwork[i] = unitNetworkData;
            }
        }

        SetHasUpdatedServerRPC();
    }

    [ServerRpc]
    private void SetHasUpdatedServerRPC()
    {
        HasUpdatedUnitsOnBoard.Value = true;
    }

    #region Utility

    public List<Unit> GetSurroundingUnits(Unit unit)
    {
        if (unit == null || unit.CurrentTile == null)
        {
            Debug.LogError("Unit is null");

            return null;
        }

        Vector2Int index = unit.CurrentTile.Index;

        List<Vector2Int> neighbourIndices = PathFinding.GetNeighbours(index, Tiles);

        List<Unit> units = new List<Unit>();
        for (int i = 0; i < neighbourIndices.Count; i++)
        {
            if (Tiles[neighbourIndices[i].x, neighbourIndices[i].y].CurrentUnit != null)
            {
                units.Add(Tiles[neighbourIndices[i].x, neighbourIndices[i].y].CurrentUnit);
            }
        }

        return units;
    }

    public void SellUnit(Unit unit)
    {
        unit.CurrentTile.CurrentUnit = null;
        Units.Remove(unit);

        for (int i = 0; i < unit.ItemCount; i++)
        {
            PlayerHandler.LootSystem.SpawnItem(unit.ItemSlots[i]);
        }

        print("Selling unit: " + unit);
        GameManager.Instance.DestroyServerRPC(unit.GetComponent<NetworkObject>().NetworkObjectId);

        int money = Mathf.FloorToInt(unit.UnitData.Cost * Mathf.Pow(3, unit.StarLevel));
        if (unit.UnitData.Cost > 1 && unit.StarLevel > 0) money--;
        PlayerHandler.MoneySystem.AddMoney(money);
    }

    public void UpdateBoardUnits()
    {
        OnBoardedUnitsChanged?.Invoke(UnitsOnBoard);
    }

    #endregion
}
