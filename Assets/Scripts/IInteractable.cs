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
    /// <returns>If pickup succeded</returns>
    public bool Pickup();

    /// <summary>
    /// 
    /// </summary>
    /// <returns>If placing succeded</returns>
    public bool Place();
}
