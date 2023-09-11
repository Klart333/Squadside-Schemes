using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InlineEditor]
[CreateAssetMenu(fileName = "New Helper", menuName = "Unit/Unit Data Utility")]
public class UnitDataUtility : ScriptableObject
{
    public List<UnitData> AllUnits = new List<UnitData>();

    public int[] CostCounts;

    [Button]
    public void Sort()
    {
        AllUnits.Sort((x, y) => x.Cost.CompareTo(y.Cost));

        CostCounts = new int[5];
        int count = 0;
        int index = 0;
        for (int i = 0; i < AllUnits.Count; i++)
        {
            if (i == 0 || AllUnits[i].Cost == AllUnits[i - 1].Cost)
            {
                count++;
            }
            else
            {
                CostCounts[index++] = count;
                count = 1;
            }
        }
        CostCounts[index] = count;

        Debug.Log("Sorted");
    }

    public UnitData GetUnit(float[] odds)
    {
        float value = Random.value;
        float chance = 0;
        int index = 0;
        for (int i = 0; i < odds.Length; i++)
        {
            chance += odds[i];
            if (value <= chance)
            {
                index = i;
                break;
            }
        }

        int startIndex = 0;
        for (int i = index - 1; i >= 0; i--)
        {
            startIndex += CostCounts[i];
        }
        int stopIndex = startIndex + CostCounts[index];

        return AllUnits[Random.Range(startIndex, stopIndex)];
    }

    public int GetIndex(UnitData unitData)
    {
        if (AllUnits.Contains(unitData))
        {
            return AllUnits.IndexOf(unitData);
        }

        Debug.LogError("Could not find UnitData in all units");
        return -1;
    }

    public UnitData Get(int index)
    {
        if (index >= 0 && index < AllUnits.Count)
        {
            return AllUnits[index];
        }

        Debug.LogError("Argument out of range exception");
        return null;
    }
}
