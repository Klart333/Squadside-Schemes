using UnityEngine;

public interface IInteractable
{
    public GameObject gameObject { get; }
    public bool IsOwner { get; }
    public bool IsInteractable { get; }

    public void StartInteract();
    public void EndInteract();
    /// <summary>
    /// 
    /// </summary>
    /// <returns>If it's a click</returns>
    public bool Pickup();
    public void Place();
}
