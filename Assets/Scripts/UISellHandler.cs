using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISellHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private PlayerUI playerUI;

    [SerializeField]
    private TextMeshProUGUI sellText;

    [SerializeField]
    private Image outline;

    private Transform background;

    private bool mouseOver = false;

    private async void Start()
    {
        background = transform.GetChild(0);
        background.gameObject.SetActive(false);
        outline.gameObject.SetActive(false);

        await UniTask.WaitUntil(() => playerUI.PlayerHandler.BoardSystem != null);

        playerUI.PlayerHandler.BoardSystem.OnUnitPickup += BoardSystem_OnUnitPickup;
        playerUI.PlayerHandler.BoardSystem.OnUnitPlace += BoardSystem_OnUnitPlace;
    }
    private void OnDestroy()
    {
        if (!playerUI || !playerUI.PlayerHandler || !playerUI.PlayerHandler.BoardSystem) return;

        playerUI.PlayerHandler.BoardSystem.OnUnitPickup += BoardSystem_OnUnitPickup;
        playerUI.PlayerHandler.BoardSystem.OnUnitPlace -= BoardSystem_OnUnitPlace;
    }

    private void BoardSystem_OnUnitPickup(Unit unit)
    {
        background.gameObject.SetActive(true);

        int money = Mathf.FloorToInt(unit.UnitData.Cost * Mathf.Pow(3, unit.StarLevel));
        if (unit.UnitData.Cost > 1 && unit.StarLevel > 0) money--;
        sellText.text = string.Format("Sell {0}g", money);
    }

    private void BoardSystem_OnUnitPlace(Unit unit)
    {
        print("SELL UNIT!!");

        background.gameObject.SetActive(false);

        if (mouseOver)
        {
            playerUI.PlayerHandler.BoardSystem.SellUnit(unit);
        }

        mouseOver = false;
        outline.gameObject.SetActive(false);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
        outline.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
        outline.gameObject.SetActive(true);
    }
}
