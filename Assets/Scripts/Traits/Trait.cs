using Effects;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

[InlineEditor, CreateAssetMenu(fileName = "New Trait", menuName = "Unit/Trait")]
public class Trait : SerializedScriptableObject
{
    [Title("Info")]
    public string Name;

    [AssetSelector]
    public Sprite Icon;

    [Title("Trait")]
    [OdinSerialize]
    [NonSerialized]
    public TraitEffects[] TraitBreakpoints;

    public void OnBench(Unit unit, int traitCount)
    {
        for (int i = TraitBreakpoints.Length - 1; i >= 0; i--)
        {
            if (traitCount >= TraitBreakpoints[i].UnitCount)
            {
                RevertEffects(TraitBreakpoints[i].OnBoardEffects, unit);
                break;
            }
        }
    }

    public void OnBoard(Unit unit, int traitCount)
    {
        for (int i = TraitBreakpoints.Length - 1; i >= 0; i--)
        {
            if (traitCount >= TraitBreakpoints[i].UnitCount)
            {
                ApplyEffects(TraitBreakpoints[i].OnBoardEffects, unit);
                break;
            }
        }
    }

    public void OnAttack(Unit unit, int traitCount)
    {
        for (int i = TraitBreakpoints.Length - 1; i >= 0; i--)
        {
            if (traitCount >= TraitBreakpoints[i].UnitCount)
            {
                ApplyEffects(TraitBreakpoints[i].OnAttackEffects, unit);
                break;
            }
        }
    }

    public void OnTakeDamage(Unit unit, int traitCount)
    {
        for (int i = TraitBreakpoints.Length - 1; i >= 0; i--)
        {
            if (traitCount >= TraitBreakpoints[i].UnitCount)
            {
                ApplyEffects(TraitBreakpoints[i].OnAttackEffects, unit);
                break;
            }
        }
    }

    private void ApplyEffects(List<IEffect> effects, Unit unit)
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

    public void RevertAll(Unit unit)
    {
        for (int i = 0; i < TraitBreakpoints.Length; i++)
        {
            RevertEffects(TraitBreakpoints[i].OnBoardEffects, unit);
            RevertEffects(TraitBreakpoints[i].OnAttackEffects, unit);
            RevertEffects(TraitBreakpoints[i].OnTakeDamageEffects, unit);
        }
    }
}

[Serializable]
public class TraitEffects
{
    public int UnitCount = 2;

    [Title("Board")]
    [OdinSerialize]
    public List<IEffect> OnBoardEffects = new List<IEffect>();

    [Title("Attack")]
    [OdinSerialize]
    public List<IEffect> OnAttackEffects = new List<IEffect>();

    [Title("Attack")]
    [OdinSerialize]
    public List<IEffect> OnTakeDamageEffects = new List<IEffect>();
}

