using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitStats
{
    [Title("Combat", "Attack")]
    public Stat AttackDamage;
    public Stat AttackSpeed;
    public Stat AttackRange;

    [Title("Combat", "Ability")]
    public Stat AbilityPower;
    public Stat Mana;
    public Stat MaxMana;

    [Title("Combat", "Crit")]
    public Stat CritChance;
    public Stat CritMultiplier;

    [Title("Traits")]
    public HashSet<int> Traits;

    [Title("Defense")]
    public Stat Armor;
    public Stat MagicResist;

    [Title("Health")]
    public Stat MaxHealth;
    public Stat Omnivamp;

    [TitleGroup("Movement")]
    public Stat MovementSpeed;

    public void AddTrait(int traitIndex)
    {
        if (Traits.Contains(traitIndex))
        {
            return;
        }

        Traits.Add(traitIndex);
    }

    public void RemoveTrait(int traitIndex)
    {
        if (!Traits.Contains(traitIndex))
        {
            return;
        }

        Traits.Remove(traitIndex);
    }

    public void ModifyStat(StatType statType, Modifier modifier)
    {
        switch (statType)
        {
            case StatType.AttackDamage:
                AttackDamage.AddModifier(modifier);
                break;
            case StatType.AttackSpeed:
                AttackSpeed.AddModifier(modifier);
                break;
            case StatType.AttackRange:
                AttackRange.AddModifier(modifier);
                break;
            case StatType.AbilityPower:
                AbilityPower.AddModifier(modifier);
                break;
            case StatType.Mana:
                Mana.AddModifier(modifier);
                break;
            case StatType.MaxMana:
                MaxMana.AddModifier(modifier);
                break;
            case StatType.CritChance:
                CritChance.AddModifier(modifier);
                break;
            case StatType.CritMultiplier:
                CritMultiplier.AddModifier(modifier);
                break;
            case StatType.Armor:
                Armor.AddModifier(modifier);
                break;
            case StatType.MagicResist:
                MagicResist.AddModifier(modifier);
                break;
            case StatType.MaxHealth:
                MaxHealth.AddModifier(modifier);
                break;
            case StatType.Omnivamp:
                Omnivamp.AddModifier(modifier);
                break;
            case StatType.MovementSpeed:
                MovementSpeed.AddModifier(modifier);
                break;
            case StatType.ArmornMR:
                Armor.AddModifier(modifier);
                MagicResist.AddModifier(modifier);
                break;
            default:
                break;
        }
    }

    public void RevertModifiedStat(StatType statType, Modifier modifier)
    {
        switch (statType)
        {
            case StatType.AttackDamage:
                AttackDamage.RemoveModifier(modifier);
                break;
            case StatType.AttackSpeed:
                AttackSpeed.RemoveModifier(modifier);
                break;
            case StatType.AttackRange:
                AttackRange.RemoveModifier(modifier);
                break;
            case StatType.AbilityPower:
                AbilityPower.RemoveModifier(modifier);
                break;
            case StatType.Mana:
                Mana.RemoveModifier(modifier);
                break;
            case StatType.MaxMana:
                MaxMana.RemoveModifier(modifier);
                break;
            case StatType.CritChance:
                CritChance.RemoveModifier(modifier);
                break;
            case StatType.CritMultiplier:
                CritMultiplier.RemoveModifier(modifier);
                break;
            case StatType.Armor:
                Armor.RemoveModifier(modifier);
                break;
            case StatType.MagicResist:
                MagicResist.RemoveModifier(modifier);
                break;
            case StatType.MaxHealth:
                MaxHealth.RemoveModifier(modifier);
                break;
            case StatType.Omnivamp:
                Omnivamp.RemoveModifier(modifier);
                break;
            case StatType.MovementSpeed:
                MovementSpeed.RemoveModifier(modifier);
                break;
            case StatType.ArmornMR:
                Armor.RemoveModifier(modifier);
                MagicResist.RemoveModifier(modifier);
                break;
            default:
                break;
        }
    }
}

public enum StatType
{
    AttackDamage,
    AttackSpeed,
    AttackRange,
    AbilityPower,
    Mana,
    MaxMana,
    CritChance,
    CritMultiplier,
    Armor,
    MagicResist,
    MaxHealth,
    Omnivamp,
    MovementSpeed,
    ArmornMR
}

[Serializable]
public class Stat
{
    public event Action OnValueChanged;

    public float BaseValue
    {
        get
        {
            return baseValue;
        }

        set
        {
            baseValue = value;

            OnValueChanged?.Invoke();
        }
    }

    [ShowInInspector]
    private float baseValue;

    private List<Modifier> modifiers = new List<Modifier>();

    [ShowInInspector, ReadOnly]
    public float Value
    {
        get
        {
            float val = BaseValue;

            for (int i = 0; i < modifiers.Count; i++)
            {
                switch (modifiers[i].Type)
                {
                    case Modifier.ModifierType.Multiplicative:
                        val *= modifiers[i].Value;
                        break;
                    case Modifier.ModifierType.Additive:
                        val += modifiers[i].Value;
                        break;
                    default:
                        break;
                }
            }

            return val;
        }
    }

    public Stat(float baseValue)
    {
        this.BaseValue = baseValue;
    }

    public void AddModifier(Modifier mod)
    {
        modifiers.Add(mod);

        modifiers.Sort((x, y) => x.Type.CompareTo(y.Type));

        OnValueChanged?.Invoke();
    }

    public void AddModifier(float additiveValue)
    {
        bool added = false;
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].Type == Modifier.ModifierType.Additive)
            {
                modifiers[i].Value += additiveValue;

                added = true;
                break;
            }
        }

        if (!added)
        {
            AddModifier(new Modifier { Value = additiveValue, Type = Modifier.ModifierType.Additive });
            return;
        }

        OnValueChanged?.Invoke();
    }

    public void RemoveModifier(Modifier mod)
    {
        if (!modifiers.Contains(mod))
        {
            RemoveModifier(mod.Value, mod.Type);
            return;
        }

        modifiers.Remove(mod);
        modifiers.Sort((x, y) => x.Type.CompareTo(y.Type));

        OnValueChanged?.Invoke();
    }

    public void RemoveModifier(float value, Modifier.ModifierType type)
    {
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].Type == type && Mathf.Abs(modifiers[i].Value - value) < Mathf.Epsilon)
            {
                modifiers.RemoveAt(i);
                return;
            }
        }
    }

    public void RemoveAllModifiers()
    {
        modifiers.Clear();
        OnValueChanged?.Invoke();
    }
}

[Serializable]
public class Modifier
{
    public enum ModifierType
    {
        Additive = 0,
        Multiplicative = 1,
    }

    public ModifierType Type;

    public float Value;
}