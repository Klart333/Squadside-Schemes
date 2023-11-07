using Sirenix.OdinInspector;
using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    [Title("Data")]
    [SerializeField]
    private ItemData defaultItemData;

    private MeshRenderer meshRenderer;

    public bool IsInteractable => true;
    public bool IsOwner => true;

    public ItemTile CurrentTile { get; set; }
    public ItemData ItemData { get; set; }
    public PlayerHandler PlayerHandler { get; set; }

    private void Start()
    {
        if (ItemData == null)
        {
            ItemData = defaultItemData;
        }

        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public void SetItemData(ItemData itemData)
    {
        this.ItemData = itemData;

        var rends = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            rends[i].sprite = itemData.Icon;
        }
    }

    #region Interact

    public void StartInteract()
    {
        if (!meshRenderer)
        {
            return;
        }

        meshRenderer.gameObject.layer = LayerMask.NameToLayer("Highlight");
    }

    public void EndInteract()
    {
        if (!meshRenderer)
        {
            return;
        }

        meshRenderer.gameObject.layer = LayerMask.NameToLayer("Item");
    }

    public bool Pickup()
    {
        PlayerHandler.LootSystem.PlacingItem(this);

        return true;
    }

    public bool Place()
    {
        PlayerHandler.LootSystem.PlaceItem(this);

        return true; // The item is always placed down
    }

    #endregion
}