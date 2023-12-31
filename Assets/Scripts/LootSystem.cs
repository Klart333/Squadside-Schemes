using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class LootSystem : MonoBehaviour
{
    [Title("Orbs")]
    [SerializeField]
    private LootOrb lootOrbPrefab;

    [Title("Probability")]
    [SerializeField]
    private float lootProbability;

    [SerializeField]
    private float emblemProbablity;

    [SerializeField]
    private float moneyProbability;

    [SerializeField]
    private Vector2 moneyAmountRange;

    [Title("Items")]
    [SerializeField]
    private Item itemPrefab;

    [SerializeField]
    private Net netPrefab;

    [Title("Cached")]
    [SerializeField]
    private List<Vector3> tilePositions;

    private List<ItemTile> tiles = new List<ItemTile>();
    private List<Net> nets = new List<Net>();

    private Camera cam;
    private Net highlightedNet;
    private Unit currentHighlightedUnit;

    private bool netVisible = false;

    public void GetTilePositions()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            tilePositions.Add(transform.GetChild(i).transform.localPosition);
        }
    }

    public List<ItemTile> Tiles => tiles;
    public PlayerHandler PlayerHandler { get; set; }

    private void Start()
    {
        cam = Camera.main;

        GenerateTiles();
        ToggleNet(false);
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
            ItemTile tile = GetClosestTile(pos);
            Net net = nets[tile.Index];

            if (hit.transform.gameObject.TryGetComponent(out Unit unit))
            {
                if (highlightedNet != null)
                {
                    highlightedNet.ResetNet();
                }

                unit.StartInteract();
                currentHighlightedUnit = unit;

                return;
            }

            if (currentHighlightedUnit != null)
            {
                currentHighlightedUnit.EndInteract();
            }

            currentHighlightedUnit = null;

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

    private void GenerateTiles()
    {
        for (int i = 0; i < tilePositions.Count; i++)
        {
            Net net = Instantiate(netPrefab, transform);
            net.transform.localPosition = tilePositions[i];
            Quaternion rot = Quaternion.AngleAxis(Random.Range(-20, 20), Vector3.up);
            net.transform.localRotation = rot;
            nets.Add(net);

            ItemTile tile = new ItemTile
            {
                WorldPosition = transform.position + tilePositions[i] * transform.localScale.x,
                Index = i,
                Rotation = rot,
            };
            tiles.Add(tile);
        }
    }

    public void MaybeSpawnLoot(Vector3 pos)
    {
        float value = Random.value;

        if (value > lootProbability)
        {
            return;
        }

        LootOrb orb = lootOrbPrefab.GetAtPosAndRot<LootOrb>(pos + Vector3.up, Quaternion.identity);

        float moneyRandValue = Random.value;
        if (moneyRandValue < moneyProbability)
        {
            int moneyAmount = Mathf.RoundToInt(Random.Range(moneyAmountRange.x, moneyAmountRange.y));
            orb.MoneyAmount = moneyAmount;
            orb.MoneySystem = PlayerHandler.MoneySystem;
        }
        else
        {
            float itemValue = Random.value;
            if (itemValue < emblemProbablity)
            {
                orb.LootItemData = GameManager.Instance.ItemDataUtility.GetRandomEmblem();
            }
            else
            {
                orb.LootItemData = GameManager.Instance.ItemDataUtility.GetRandomItem();
            }

            orb.LootSystem = this;
        }
    }

    public void SpawnItem(ItemData itemData)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].CurrentItem != null)
            {
                continue;
            }

            Item item = Instantiate(itemPrefab, tiles[i].WorldPosition, tiles[i].Rotation);
            item.SetItemData(itemData);
            item.PlayerHandler = this.PlayerHandler;

            SetItemToTile(item, tiles[i]);

            break;
        }
    }

    public void PlacingItem(Item item)
    {
        ToggleNet(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns>If the item was placed on a Unit</returns>
    public bool PlaceItem(Item item)
    {
        ToggleNet(false);

        // Check first if its on a unit
        if (currentHighlightedUnit != null)
        {
            currentHighlightedUnit.EndInteract();

            if ((currentHighlightedUnit.IsOnBoard && PlayerHandler.InteractionRestricted) || !currentHighlightedUnit.ApplyItem(item.ItemData)) // Check if placing an item is allowed
            {
                SetItemToTile(item, item.CurrentTile);
                currentHighlightedUnit = null;
                return false;
            }

            currentHighlightedUnit = null;
            item.CurrentTile.CurrentItem = null;

            Destroy(item.gameObject);
            return true;
        }

        // If not
        ItemTile closestTile = tiles[nets.IndexOf(highlightedNet)];

        if (Vector3.Distance(closestTile.WorldPosition, item.transform.position) > 3)
        {
            SetItemToTile(item, item.CurrentTile);
            return false;
        }

        if (closestTile.CurrentItem != null)
        {
            SetItemToTile(closestTile.CurrentItem, item.CurrentTile);
        }
        else
        {
            item.CurrentTile.CurrentItem = null;
        }

        SetItemToTile(item, closestTile);
        return false;
    }

    private void SetItemToTile(Item item, ItemTile tile)
    {
        item.transform.position = tile.WorldPosition;
        item.transform.rotation = tile.Rotation;

        item.CurrentTile = tile;
        tile.CurrentItem = item;
    }

    private ItemTile GetClosestTile(Vector3 position)
    {
        float minDist = float.MaxValue;
        int index = 0;
        for (int i = 0; i < tiles.Count; i++)
        {
            float dist = (position - tiles[i].WorldPosition).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                index = i;
            }
        }

        return tiles[index];
    }

    private void ToggleNet(bool enabled)
    {
        netVisible = enabled;
        for (int i = 0; i < nets.Count; i++)
        {
            nets[i].ResetNet();
            nets[i].gameObject.SetActive(enabled);
        }
    }
}

public class ItemTile
{
    public Quaternion Rotation;
    public Vector3 WorldPosition;
    public int Index;
    public Item CurrentItem;
}