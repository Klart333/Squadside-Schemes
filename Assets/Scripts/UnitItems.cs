using UnityEngine;
using UnityEngine.UI;

public class UnitItems : MonoBehaviour
{
    [SerializeField]
    private Image[] itemSlotImages;

    private Unit unit;

    private void Start()
    {
        unit = GetComponent<Unit>();

        unit.OnAttack += Unit_OnAttack;
        unit.OnDamageDone += Unit_OnDamageDone;
        unit.OnKill += Unit_OnKill;
        unit.OnTakeDamage += Unit_OnTakeDamage;
    }

    private void OnDestroy()
    {
        unit.OnAttack -= Unit_OnAttack;
        unit.OnDamageDone -= Unit_OnDamageDone;
        unit.OnKill -= Unit_OnKill;
        unit.OnTakeDamage -= Unit_OnTakeDamage;
    }

    private void Unit_OnTakeDamage()
    {
        for (int i = 0; i < unit.ItemSlots.Length; i++)
        {
            if (unit.ItemSlots[i] == null)
            {
                break;
            }

            unit.ItemSlots[i].ApplyEffects(unit.ItemSlots[i].OnTakeDamageEffects, unit);
        }
    }

    private void Unit_OnKill()
    {
        for (int i = 0; i < unit.ItemSlots.Length; i++)
        {
            if (unit.ItemSlots[i] == null)
            {
                break;
            }

            unit.ItemSlots[i].ApplyEffects(unit.ItemSlots[i].OnKillEffects, unit);
        }
    }

    private void Unit_OnDamageDone()
    {
        for (int i = 0; i < unit.ItemSlots.Length; i++)
        {
            if (unit.ItemSlots[i] == null)
            {
                break;
            }

            unit.ItemSlots[i].ApplyEffects(unit.ItemSlots[i].OnDamageDone, unit);
        }
    }

    private void Unit_OnAttack()
    {
        for (int i = 0; i < unit.ItemSlots.Length; i++)
        {
            if (unit.ItemSlots[i] == null)
            {
                break;
            }

            unit.ItemSlots[i].ApplyEffects(unit.ItemSlots[i].OnAttackEffects, unit);
        }
    }

    public void ApplyItem(ItemData item, int index)
    {
        item.ApplyEffects(item.OnBoardEffects, unit);

        itemSlotImages[index].enabled = true;
        itemSlotImages[index].sprite = item.Icon;
    }

    public void RemoveItem(ItemData item, int index)
    {
        item.RevertAll(unit);

        itemSlotImages[index].enabled = false;
        itemSlotImages[index].sprite = null;
    }
}
