﻿using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitStats
{
    [Title("Combat")]
    public Stat AttackDamage;
    public Stat AttackSpeed;
    public Stat AttackRange;

    public Stat AbilityPower;
    public Stat Mana;
    public Stat MaxMana;

    [Title("Traits")]
    public HashSet<int> Traits;

    [Title("Health")]
    public Stat MaxHealth;

    [TitleGroup("Movement")]
    public Stat MovementSpeed;

    public void AddTrait(int traitIndex)
    {
        Traits.Add(traitIndex);
    }

    public void RemoveTrait(int traitIndex)
    {
        Traits.Remove(traitIndex);
    }
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