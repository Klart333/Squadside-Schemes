using DG.Tweening;
using UnityEngine;

public class LootOrb : PooledMonoBehaviour, IInteractable
{
    private MeshRenderer meshRenderer;

    private Vector3 startScale;

    public LootSystem LootSystem { get; set; }
    public ItemData LootItemData { get; set; }
    public MoneySystem MoneySystem { get; set; }

    public int MoneyAmount { get; set; }
    public bool IsOwner => true;
    public bool IsInteractable => true;

    private void Start()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        startScale = transform.localScale;
    }

    #region Interact

    public void StartInteract()
    {
        if (!meshRenderer)
        {
            return;
        }

        meshRenderer.gameObject.layer = LayerMask.NameToLayer("Highlight");

        transform.DOScale(startScale.x * 1.2f, 1).SetEase(Ease.OutCirc);
    }

    public void EndInteract()
    {
        if (!meshRenderer)
        {
            return;
        }

        meshRenderer.gameObject.layer = LayerMask.NameToLayer("Item");

        transform.DOKill();
        transform.DOScale(startScale.x, 0.75f).SetEase(Ease.OutCirc);
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

        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.UIDeepClick);
        Reset();
        gameObject.SetActive(false);

        return false;
    }

    private void Reset()
    {
        LootSystem = null;
        MoneySystem = null;
        LootItemData = null;
        MoneyAmount = 0;
    }

    public bool Place()
    {
        return false;
    }

    #endregion
}
