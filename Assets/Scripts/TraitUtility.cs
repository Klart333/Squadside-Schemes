﻿using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[InlineEditor]
[CreateAssetMenu(fileName = "Trait Utility", menuName = "Unit/Trait Utility")]
public class TraitUtility : ScriptableObject
{
    public List<Trait> AllTraits = new List<Trait>();

    public List<Trait> GetTraits(List<int> indices)
    {
        List<Trait> traits = new List<Trait>(indices.Count);

        for (int i = 0; i < indices.Count; i++)
        {
            traits.Add(AllTraits[indices[i]]);
        }

        return traits;
    }

    public List<int> GetIndices(IEnumerable<Trait> traits)
    {
        List<int> indices = new List<int>();

        foreach (var trait in traits)
        {
            if (AllTraits.Contains(trait))
            {
                indices.Add(AllTraits.IndexOf(trait));
            }
        }

        return indices;
    }

    public int GetIndex(Trait trait)
    {
        if (AllTraits.Contains(trait))
        {
            return AllTraits.IndexOf(trait);
        }

        Debug.LogError("Could not find the trait");
        return -1;
    }

    public Trait Get(int index)
    {
        if (index >= 0 && index < AllTraits.Count)
        {
            return AllTraits[index];
        }

        Debug.LogError("Argument out of range exception");
        return null;
    }

    public Dictionary<Trait, int> GetTraits(List<Unit> units)
    {
        Dictionary<Trait, int> traits = new Dictionary<Trait, int>();
        HashSet<string> countedUnits = new HashSet<string>();

        for (int i = 0; i < units.Count; i++)
        {
            if (countedUnits.Contains(units[i].UnitData.Name))
            {
                continue;
            }

            countedUnits.Add(units[i].UnitData.Name);

            foreach (int traitIndex in units[i].UnitStats.Traits)
            {
                Trait trait = Get(traitIndex);
                if (traits.ContainsKey(trait))
                {
                    traits[trait]++;
                }
                else
                {
                    traits.Add(trait, 1);
                }
            }
        }

        return traits;
    }
}
