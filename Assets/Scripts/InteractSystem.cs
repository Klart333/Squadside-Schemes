using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class InteractSystem : NetworkBehaviour
{
    [TitleGroup("Raycast")]
    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private LayerMask placingLayerMask;

    private Camera cam;

    private IInteractable currentInteractable;

    private bool holding;

    public PlayerHandler PlayerHandler { get; set; }
    public IInteractable CurrentInteractable => currentInteractable;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (!holding)
        {
            HandleInteract();
        }
        else if (CurrentInteractable != null && CurrentInteractable.gameObject != null)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100, placingLayerMask))
            {
                Vector3 pos = hit.point;
                CurrentInteractable.gameObject.transform.position = pos;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (holding)
            {
                Place();
            }
            else
            {
                Pickup();
            }
        }
    }

    private void Place()
    {
        holding = false;

        currentInteractable?.Place();
    }

    private void Pickup()
    {
        if (currentInteractable == null)
        {
            return;
        }

        holding = true;
        currentInteractable.Pickup();
    }

    private void HandleInteract()
    {
        IInteractable interact = GetRaycastInteractable();

        if (interact == null)
        {
            if (currentInteractable != null)
            {
                currentInteractable.EndInteract();
            }

            currentInteractable = null;
            return;
        }

        if (!interact.IsInteractable || interact == currentInteractable)
        {
            return;
        }

        if (currentInteractable != null)
        {
            currentInteractable.EndInteract();
        }

        currentInteractable = interact;

        currentInteractable.StartInteract();
    }

    public IInteractable GetRaycastInteractable()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100, layerMask))
        {
            if (!hit.transform.gameObject.TryGetComponent(out IInteractable interact) || !interact.IsOwner)
            {
                return null;
            }

            return interact;
        }

        return null;
    }
}