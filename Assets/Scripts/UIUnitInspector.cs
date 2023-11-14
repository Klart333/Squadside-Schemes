using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitInspector : MonoBehaviour
{
    [Title("Portrait", "Portrait")]
    [SerializeField]
    private Image portraitImage;

    [SerializeField]
    private TextMeshProUGUI unitName;

    [SerializeField]
    private TextMeshProUGUI unitCost;

    [Title("Portrait", "Trait")]
    [SerializeField]
    private TextMeshProUGUI[] traitTexts;

    [SerializeField]
    private Image[] traitIcons;

    [Title("Health")]
    [SerializeField]
    private TextMeshProUGUI healthText;

    [SerializeField]
    private Image healthFill;

    [Title("Mana")]
    [SerializeField]
    private TextMeshProUGUI manaText;

    [SerializeField]
    private Image manaFill;

    [Title("Stats")]
    [SerializeField]
    private TextMeshProUGUI ADText;

    [SerializeField]
    private TextMeshProUGUI APText;

    [SerializeField]
    private TextMeshProUGUI ArmorText;

    [SerializeField]
    private TextMeshProUGUI MRText;

    [SerializeField]
    private TextMeshProUGUI ASText;

    [SerializeField]
    private TextMeshProUGUI CritText;

    [SerializeField]
    private TextMeshProUGUI HealingText;

    private void Update()
    {
        if (InputManager.Instance.Fire.WasPerformedThisFrame())
        {
            gameObject.SetActive(false);
        }
    }

    public void DisplayUnit(Unit unit)
    {
        // Portrait
        portraitImage.sprite = unit.UnitData.Portrait;
        unitName.text = unit.UnitData.Name;
        unitCost.text = string.Format("{0}g", unit.UnitData.Cost);

        DisableTraits();

        int length = Mathf.Min(unit.UnitData.Traits.Length, traitTexts.Length);
        for (int i = 0; i < length; i++)
        {
            traitTexts[i].gameObject.SetActive(true);
            traitTexts[i].text = unit.UnitData.Traits[i].Name;

            traitIcons[i].transform.parent.gameObject.SetActive(true);
            traitIcons[i].sprite = unit.UnitData.Traits[i].Icon;
        }

        // Health & Mana
        healthText.text = string.Format("{0} / {1}", unit.UnitHealth.CurrentHealth.ToString("f0"), unit.UnitStats.MaxHealth.Value);
        healthFill.fillAmount = unit.UnitHealth.HealthPercentage;

        manaText.text = string.Format("{0} / {1}", unit.UnitStats.Mana.Value.ToString("f0"), unit.UnitStats.MaxMana.Value);
        manaFill.fillAmount = unit.UnitStats.Mana.Value / unit.UnitStats.MaxMana.Value;

        // Stats
        ADText.text = unit.UnitStats.AttackDamage.Value.ToString();
        APText.text = unit.UnitStats.AbilityPower.Value.ToString();
        ArmorText.text = unit.UnitStats.Armor.Value.ToString();
        MRText.text = unit.UnitStats.MagicResist.Value.ToString();
        ASText.text = unit.UnitStats.AttackSpeed.Value.ToString();
        CritText.text = unit.UnitStats.CritChance.Value.ToString();
        HealingText.text = unit.UnitStats.Omnivamp.Value.ToString();
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

}
