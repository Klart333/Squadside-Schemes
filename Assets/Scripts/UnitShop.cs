using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitShop : MonoBehaviour
{
    [Title("Setup")]
    [SerializeField]
    private Image background;

    [SerializeField]
    private Image portrait;

    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI costText;

    [Title("Trait", "Text")]
    [SerializeField]
    private TextMeshProUGUI[] traitTexts;

    [Title("Trait", "Icon")]
    [SerializeField]
    private Image[] traitIcons;

    [Title("Colors")]
    [SerializeField]
    private Color[] costColors;

    private UnitData currentUnitData;
    private Button button;

    private Color transparent;
    private bool bought = false;

    public PlayerUI PlayerUI { get; set; }

    public void Initialize()
    {
        transparent = new Color(0, 0, 0, 0);

        button = GetComponent<Button>();

        PlayerUI.PlayerHandler.MoneySystem.OnAmountChanged += CheckHasEnough;
    }

    private void OnDisable()
    {
        if (!PlayerUI)
        {
            return;
        }

        PlayerUI.PlayerHandler.MoneySystem.OnAmountChanged -= CheckHasEnough;
    }

    private void CheckHasEnough()
    {
        if (currentUnitData == null)
        {
            return;
        }

        button.enabled = PlayerUI.PlayerHandler.MoneySystem.HasEnough(currentUnitData.Cost);
    }

    public void Setup(UnitData unitData)
    {
        bought = false;

        background.color = costColors[unitData.Cost - 1];
        for (int i = 0; i < background.transform.childCount; i++)
        {
            background.transform.GetChild(i).gameObject.SetActive(true);
        }

        portrait.sprite = unitData.Portrait;
        portrait.color = Color.white;

        nameText.text = unitData.Name;

        costText.text = $"{unitData.Cost}g";

        currentUnitData = unitData;

        DisableTraits();

        int length = Mathf.Min(unitData.Traits.Length, traitTexts.Length);
        for (int i = 0; i < length; i++)
        {
            traitTexts[i].gameObject.SetActive(true);
            traitTexts[i].text = unitData.Traits[i].Name;

            traitIcons[i].transform.parent.gameObject.SetActive(true);
            traitIcons[i].sprite = unitData.Traits[i].Icon;
        }

        CheckHasEnough();
    }

    private void DisableTraits()
    {
        for (int i = 0; i < traitTexts.Length; i++)
        {
            traitTexts[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < traitIcons.Length; i++)
        {
            traitIcons[i].transform.parent.gameObject.SetActive(false);
        }
    }

    public void BuyUnit()
    {
        if (bought)
        {
            return;
        }

        if (!PlayerUI.PlayerHandler.BoardSystem.SpawnUnit(currentUnitData))
        {
            return;
        }

        PlayerUI.PlayerHandler.MoneySystem.RemoveMoney(currentUnitData.Cost);

        bought = true;
        SetEmpty();
    }

    public void SetEmpty()
    {
        for (int i = 0; i < background.transform.childCount; i++)
        {
            background.transform.GetChild(i).gameObject.SetActive(false);
        }

        background.color = transparent;
        portrait.color = transparent;

        nameText.text = "";
        costText.text = "";
    }
}
