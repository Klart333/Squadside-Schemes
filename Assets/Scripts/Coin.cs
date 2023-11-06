using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class Coin : PooledMonoBehaviour, IInteractable
{
    [Title("Animation")]
    [SerializeField]
    private Vector2Int turnAmountRange;

    [SerializeField]
    private Vector2 heightRange;

    [SerializeField]
    private Vector2 speedRange;

    private MeshRenderer meshRenderer;

    public MoneySystem MoneySystem { get; set; }
    public bool IsOwner => true;
    public bool IsInteractable => true;

    private void Start()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

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
        if (MoneySystem != null)
        {
            MoneySystem.AddMoney(1);
        }

        gameObject.SetActive(false);
        return true;
    }

    public void Place()
    {

    }

    public async void AnimateToPosition(Vector3 targetPos)
    {
        float height = Random.Range(heightRange.x, heightRange.y);

        Vector3 startPosition = transform.position;
        Vector3 middlePosition = Vector3.Lerp(startPosition, targetPos, 0.5f) + Vector3.up * height;

        float t = 0;
        float speed = Random.Range(speedRange.x, speedRange.y);

        Rotate(speed);
        while (t < 1.0f)
        {
            t += Time.deltaTime * speed;

            transform.position = Vector3.Lerp(Vector3.Lerp(startPosition, middlePosition, t), Vector3.Lerp(middlePosition, targetPos, t), Math.EaseOutCubic(t));

            await UniTask.Yield();
        }
    }

    public async void Rotate(float speed)
    {
        int turns = Random.Range(turnAmountRange.x, turnAmountRange.y);

        float totalAngle = 90 + 360 * turns;
        float rotationSpeed = totalAngle * speed;

        while (totalAngle > 0)
        {
            float subRotationAngle = Mathf.Min(totalAngle, rotationSpeed * Time.deltaTime);

            Quaternion subTargetRotation = Quaternion.AngleAxis(subRotationAngle, Vector3.forward);

            transform.rotation *= subTargetRotation;

            totalAngle -= subRotationAngle;

            await UniTask.Yield();
        }
    }
}

public static class Math
{
    public static float EaseOutCubic(float x)
    {
        return 1.0f - Mathf.Pow(1.0f - x, 3.0f);
    }
}