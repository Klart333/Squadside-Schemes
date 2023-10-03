using UnityEngine;

public class LootOrb : MonoBehaviour, IInteractable
{
    private MeshRenderer meshRenderer;

    public LootSystem LootSystem { get; set; }
    public ItemData LootItemData { get; set; }
    public bool IsOwner => true;
    public bool IsInteractable => true;

    private void Start()
    {
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
        print(LootSystem);
        LootSystem.SpawnItem(LootItemData);

        Destroy(gameObject);
        return true;
    }

    public void Place()
    {

    }

    #endregion
}
