using Effects;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

[InlineEditor]
[CreateAssetMenu(fileName = "Item Data", menuName = "Item/Item Data")]
public class ItemData : SerializedScriptableObject, IComparable<ItemData>
{
    [Title("Item")]
    public Sprite Icon;

    [TextArea]
    public string Description;

    public bool IsComponent;

    [Title("Effect")]
    [OdinSerialize]
    public List<IEffect> OnBoardEffects = new List<IEffect>();

    [OdinSerialize]
    public List<IEffect> OnTakeDamageEffects = new List<IEffect>();

    [OdinSerialize]
    public List<IEffect> OnAttackEffects = new List<IEffect>();

    [OdinSerialize]
    public List<IEffect> OnDamageDone = new List<IEffect>();

    [OdinSerialize]
    public List<IEffect> OnKillEffects = new List<IEffect>();

    public void ApplyEffects(List<IEffect> effects, Unit unit)
    {
        if (effects == null || effects.Count == 0)
        {
            return;
        }

        for (int i = 0; i < effects.Count; i++)
        {
            effects[i].Perform(unit);
        }
    }

    public void RevertAll(Unit unit)
    {
        RevertEffects(OnBoardEffects, unit);
        RevertEffects(OnAttackEffects, unit);
        RevertEffects(OnKillEffects, unit);
        RevertEffects(OnTakeDamageEffects, unit);
    }

    private void RevertEffects(List<IEffect> effects, Unit unit)
    {
        if (effects == null || effects.Count == 0)
        {
            return;
        }

        for (int i = 0; i < effects.Count; i++)
        {
            effects[i].Revert(unit);
        }
    }

    public int CompareTo(ItemData other)
    {
        return name == other.name ? 0 : 1;
    }
}