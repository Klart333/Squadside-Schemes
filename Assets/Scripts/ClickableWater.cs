using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableWater : MonoBehaviour
{
    [SerializeField]
    private PooledMonoBehaviour particle;

    [SerializeField]
    private SimpleAudioEvent sfx;

    private Camera cam;
    private EventSystem eventSystem;

    private void Start()
    {
        cam = Camera.main;
        eventSystem = EventSystem.current;

        if (cam == null)
        {
            cam = FindAnyObjectByType<Camera>();
        }
    }

    private void Update()
    {
        if (InputManager.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!InputManager.Instance.Fire.WasPerformedThisFrame())
        {
            return;
        }

        if (eventSystem.IsPointerOverGameObject())
        {
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100) && hit.collider.gameObject == gameObject)
        {
            SpawnWaterAtLocation(hit.point);
        }
    }

    private void SpawnWaterAtLocation(Vector3 point)
    {
        particle.GetAtPosAndRot<PooledMonoBehaviour>(point, Quaternion.identity);
        AudioManager.Instance.PlaySoundEffect(sfx);
    }
}
