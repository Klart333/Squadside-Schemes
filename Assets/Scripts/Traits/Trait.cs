using Effects;
using Sirenix.OdinInspector;
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

    [TextArea]
    public string Description;

    [Title("Settings")]
    public bool Exclusive = false;

    [Title("Trait")]
    [OdinSerialize, NonSerialized]
    public IEffect StaticEffect;

    [OdinSerialize]
    [NonSerialized]
    public TraitEffects[] TraitBreakpoints;

    public void OnBench(Unit unit, int traitCount)
    {
        if (StaticEffect != null)
        {
            StaticEffect.Revert(unit);
        }

        for (int i = TraitBreakpoints.Length - 1; i >= 0; i--)
        {
            if (Exclusive)
            {
                if (traitCount == TraitBreakpoints[i].UnitCount)
                {
                    RevertEffects(TraitBreakpoints[i].OnBoardEffects, unit);
                    break;
                }

                continue;
            }

            if (traitCount >= TraitBreakpoints[i].UnitCount)
            {
                RevertEffects(TraitBreakpoints[i].OnBoardEffects, unit);
                break;
            }
        }
    }

    public void OnBoard(Unit unit, int traitCount)
    {
        if (StaticEffect != null)
        {
            StaticEffect.Perform(unit);
        }

        for (int i = TraitBreakpoints.Length - 1; i >= 0; i--)
        {
            if (Exclusive)
            {
                if (traitCount == TraitBreakpoints[i].UnitCount)
                {
                    ApplyEffects(TraitBreakpoints[i].OnBoardEffects, unit);
                    break;
                }

                continue;
            }

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
            if (Exclusive)
            {
                if (traitCount == TraitBreakpoints[i].UnitCount)
                {
                    ApplyEffects(TraitBreakpoints[i].OnAttackEffects, unit);
                    break;
                }

                continue;
            }

            if (traitCount >= TraitBreakpoints[i].UnitCount)
            {
                ApplyEffects(TraitBreakpoints[i].OnAttackEffects, unit);
                break;
            }
        }
    }

    public void OnKill(Unit unit, int traitCount)
    {
        for (int i = TraitBreakpoints.Length - 1; i >= 0; i--)
        {
            if (Exclusive)
            {
                if (traitCount == TraitBreakpoints[i].UnitCount)
                {
                    ApplyEffects(TraitBreakpoints[i].OnKillEffects, unit);
                    break;
                }
                continue;
            }

            if (traitCount >= TraitBreakpoints[i].UnitCount)
            {
                ApplyEffects(TraitBreakpoints[i].OnKillEffects, unit);
                break;
            }
        }
    }

    public void OnTakeDamage(Unit unit, int traitCount)
    {
        for (int i = TraitBreakpoints.Length - 1; i >= 0; i--)
        {
            if (Exclusive)
            {
                if (traitCount == TraitBreakpoints[i].UnitCount)
                {
                    ApplyEffects(TraitBreakpoints[i].OnTakeDamageEffects, unit);
                    break;
                }
                continue;
            }

            if (traitCount >= TraitBreakpoints[i].UnitCount)
            {
                ApplyEffects(TraitBreakpoints[i].OnTakeDamageEffects, unit);
                break;
            }
        }
    }

    public void OnDoneDamage(Unit unit, int traitCount)
    {
        for (int i = TraitBreakpoints.Length - 1; i >= 0; i--)
        {
            if (Exclusive)
            {
                if (traitCount == TraitBreakpoints[i].UnitCount)
                {
                    ApplyEffects(TraitBreakpoints[i].OnDoneDamageEffects, unit);
                    break;
                }
                continue;
            }

            if (traitCount >= TraitBreakpoints[i].UnitCount)
            {
                ApplyEffects(TraitBreakpoints[i].OnDoneDamageEffects, unit);
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
            RevertEffects(TraitBreakpoints[i].OnKillEffects, unit);
            RevertEffects(TraitBreakpoints[i].OnTakeDamageEffects, unit);
        }
    }

    public int GetColorIndex(int unitCount)
    {
        for (int i = TraitBreakpoints.Length - 1; i >= 0; i--)
        {
            if (Exclusive)
            {
                if (unitCount == TraitBreakpoints[i].UnitCount)
                {
                    return TraitBreakpoints[i].ColorIndex;
                }

                continue;
            }

            if (unitCount >= TraitBreakpoints[i].UnitCount)
            {
                return TraitBreakpoints[i].ColorIndex;
            }
        }

        return 0;
    }
}

[Serializable]
public class TraitEffects
{
    [Title("Trait Settings")]
    public int UnitCount = 2;
    public int ColorIndex = 1;

    [Title("Board")]
    [OdinSerialize]
    public List<IEffect> OnBoardEffects = new List<IEffect>();

    [Title("Combat")]
    [OdinSerialize]
    public List<IEffect> OnAttackEffects = new List<IEffect>();

    [OdinSerialize]
    public List<IEffect> OnDoneDamageEffects = new List<IEffect>();

    [OdinSerialize]
    public List<IEffect> OnKillEffects = new List<IEffect>();

    [OdinSerialize]
    public List<IEffect> OnTakeDamageEffects = new List<IEffect>();
}

