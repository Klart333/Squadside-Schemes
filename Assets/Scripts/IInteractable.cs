using UnityEngine;

public interface IInteractable
{
    public GameObject gameObject { get; }
    public bool IsOwner { get; }
    public bool IsInteractable { get; }

    public void StartInteract();
    public void EndInteract();
    public void Pickup();
    public void Place();
}
