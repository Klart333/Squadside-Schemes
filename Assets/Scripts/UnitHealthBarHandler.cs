using UnityEngine;

public class UnitHealthBarHandler : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam = Camera.current;
    }

    private void Update()
    {
        if (!cam)
        {
            cam = FindObjectOfType<Camera>();
        }

        Vector3 diff = cam.transform.position - transform.position;
        diff.x = 0;
        transform.rotation = Quaternion.LookRotation(diff.normalized);
    }
}
