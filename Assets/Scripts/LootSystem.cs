using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LootSystem : MonoBehaviour
{
    [Title("Orbs")]
    [SerializeField]
    private LootOrb lootOrbPrefab;

    [Title("Probability")]
    [SerializeField]
    private float lootProbability;

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

        LootOrb orb = Instantiate(lootOrbPrefab, pos + Vector3.up, Quaternion.identity);
        orb.LootItemData = GameManager.Instance.ItemDataUtility.GetRandomItem();
        orb.LootSystem = this;
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
            item.ItemData = itemData;
            item.PlayerHandler = this.PlayerHandler;

            SetItemToTile(item, tiles[i]);

            break;
        }
    }

    public void PlaceItem()
    {

    }

    public void PlacingItem(Item item)
    {
        ToggleNet(true);
    }

    public void PlaceItem(Item item)
    {
        ToggleNet(false);

        // Check first if its on a unit

        // If not
        ItemTile closestTIle = GetClosestTile(item.transform.position);

        if (Vector3.Distance(closestTIle.WorldPosition, item.transform.position) > 2)
        {
            SetItemToTile(item, item.CurrentTile);
            return;
        }

        if (closestTIle.CurrentItem != null)
        {
            SetItemToTile(closestTIle.CurrentItem, item.CurrentTile);
        }
        else
        {
            item.CurrentTile.CurrentItem = null;
        }

        SetItemToTile(item, closestTIle);
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
            float dist = Vector3.Distance(position, tiles[i].WorldPosition);
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