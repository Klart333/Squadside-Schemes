using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class ShopHandler : MonoBehaviour
{
    [TitleGroup("Setup")]
    [SerializeField]
    private PlayerUI playerUI;

    private UnitShop[] shops;

    private async void Start()
    {
        await UniTask.WaitUntil(() => GameManager.Instance != null);

        shops = GetComponentsInChildren<UnitShop>();

        for (int i = 0; i < shops.Length; i++)
        {
            shops[i].PlayerUI = this.playerUI;
            shops[i].Initialize();
        }

        RefreshShop();
    }

    public void RefreshShop()
    {
        float[] currentOdds = playerUI.PlayerHandler.LevelSystem.CurrentOdds;
        for (int i = 0; i < shops.Length; i++)
        {
            UnitData unitData = GameManager.Instance.UnitDataUtility.GetUnit(currentOdds);
            shops[i].Setup(unitData);
        }
    }
}
