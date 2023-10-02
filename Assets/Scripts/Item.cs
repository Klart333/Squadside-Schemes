using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour, IInteractable
{
    private MeshRenderer meshRenderer;

    public bool IsInteractable => true;

    public PlayerHandler PlayerHandler { get; set; }

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

    public void Pickup()
    {
        
    }

    public void Place()
    {

    }

    #endregion
}
