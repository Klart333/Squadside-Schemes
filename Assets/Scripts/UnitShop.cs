using Sirenix.OdinInspector;
using System.Collections.Generic;
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

    [Title("Highlight")]
    [SerializeField]
    private GameObject simpleHighlight;

    [SerializeField]
    private GameObject bigHighlight;

    [SerializeField]
    private GameObject[] stars;

    private UnitData currentUnitData;
    private Button button;

    private Color transparent;

    private bool bought = false;
    private bool canForceBuyUnit = false;
    private int alsoForceBuyOtherShopAtIndex = -1;

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

    public void Setup(UnitData unitData, List<Unit> unitsOnBoard, UnitData[] otherShopData, int shopIndex)
    {
        bought = false;

        background.color = costColors[unitData.Cost - 1];
        for (int i = 0; i < background.transform.childCount - 2; i++)
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

        HandleHighlight(unitData, unitsOnBoard, otherShopData, shopIndex);
    }

    private void HandleHighlight(UnitData unit, List<Unit> unitsOnBoard, UnitData[] otherShopData, int shopIndex)
    {
        canForceBuyUnit = false;
        alsoForceBuyOtherShopAtIndex = -1;

        int[] counts = new int[6];
        bool highlighted = false;
        for (int i = 0; i < unitsOnBoard.Count; i++)
        {
            if (unitsOnBoard[i].UnitData.Name == unit.Name)
            {
                highlighted = true;
                counts[unitsOnBoard[i].StarLevel]++;
            }
        }

        if (!highlighted)
        {
            simpleHighlight.SetActive(false);
            bigHighlight.SetActive(false);
            return;
        }

        int starLevel = 0;
        for (int i = 0; i < counts.Length; i++)
        {
            if (counts[i] >= 2)
            {
                starLevel++;
                canForceBuyUnit = true;
                continue;
            }
            else if (i == 0 && counts[i] == 1)
            {
                for (int g = 0; g < otherShopData.Length; g++)
                {
                    if (g == shopIndex)
                    {
                        continue;
                    }

                    if (otherShopData[g].Name == unit.Name)
                    {
                        canForceBuyUnit = true;
                        starLevel++;
                        alsoForceBuyOtherShopAtIndex = g;
                        break;
                    }
                }

                if (alsoForceBuyOtherShopAtIndex != -1)
                {
                    continue;
                }
            }

            break;
        }

        simpleHighlight.SetActive(starLevel == 0);
        bigHighlight.SetActive(starLevel > 0);
        for (int i = 0; i < stars.Length; i++)
        {
            if (starLevel == 0)
            {
                stars[i].SetActive(false);
                continue;
            }

            stars[i].SetActive(i < starLevel + 1);
        }
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
            if (!canForceBuyUnit)
            {
                return;

            }

            if (!PlayerUI.PlayerHandler.BoardSystem.SneakSpawnUnit(currentUnitData))
            {
                return;
            }

            if (alsoForceBuyOtherShopAtIndex != -1)
            {
                PlayerUI.ForceBuyUnitShop(alsoForceBuyOtherShopAtIndex);
            }
        }

        ActuallyBuyUnit();
    }

    public void ForceBuy()
    {
        if (!PlayerUI.PlayerHandler.BoardSystem.SneakSpawnUnit(currentUnitData))
        {
            return;
        }

        ActuallyBuyUnit();
    }

    private void ActuallyBuyUnit()
    {
        int index = Mathf.FloorToInt((currentUnitData.Cost - 1) / 2.0f);
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.BuyUnitSFXs[index]);

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

        canForceBuyUnit = false;
        alsoForceBuyOtherShopAtIndex = -1;
    }
}
