using System;
using UnityEngine;

public class LootOrb : PooledMonoBehaviour, IInteractable
{
    private MeshRenderer meshRenderer;

    public LootSystem LootSystem { get; set; }
    public ItemData LootItemData { get; set; }
    public MoneySystem MoneySystem { get; set; }

    public int MoneyAmount { get; set; }
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
        if (LootSystem)
        {
            LootSystem.SpawnItem(LootItemData);
        }
        else if (MoneySystem)
        {
            MoneySystem.SpawnMoney(MoneyAmount, transform.position);
        }

        Reset();
        gameObject.SetActive(false);
        return true;
    }

    private void Reset()
    {
        LootSystem = null;
        MoneySystem = null;
        LootItemData = null;
        MoneyAmount = 0;
    }

    public void Place()
    {

    }

    #endregion
}
