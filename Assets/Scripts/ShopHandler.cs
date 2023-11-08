using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopHandler : MonoBehaviour
{
    [TitleGroup("Setup")]
    [SerializeField]
    private PlayerUI playerUI;

    private UnitShop[] shops;

    private UnitData[] unitDatas;

    private async void Start()
    {
        await UniTask.WaitUntil(() => GameManager.Instance != null);

        shops = GetComponentsInChildren<UnitShop>();
        unitDatas = new UnitData[shops.Length];

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
        List<Unit> units = playerUI.PlayerHandler.BoardSystem.AllUnits;

        for (int i = 0; i < unitDatas.Length; i++)
        {
            unitDatas[i] = GameManager.Instance.UnitDataUtility.GetUnit(currentOdds);
        }

        for (int i = 0; i < shops.Length; i++)
        {
            shops[i].Setup(unitDatas[i], units, unitDatas, i);
        }
    }

    public void ForceBuyUnitShop(int index)
    {
        shops[index].ForceBuy();

    }
}
