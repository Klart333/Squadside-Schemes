using Sirenix.OdinInspector;
using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    [Title("Data")]
    [SerializeField]
    private ItemData defaultItemData;

    private MeshRenderer meshRenderer;

    public bool IsInteractable => PlayerHandler.CanInteract;
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

        return false;
    }

    public void Place()
    {
        PlayerHandler.LootSystem.PlaceItem(this);
    }

    #endregion
}