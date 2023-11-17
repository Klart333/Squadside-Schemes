using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[InlineEditor]
[CreateAssetMenu(fileName = "Item Data Utility", menuName = "Item/Item Data Utility")]
public class ItemDataUtility : SerializedScriptableObject
{
    public List<ItemData> AllItems = new List<ItemData>();

    public List<ItemData> AllEmblems = new List<ItemData>();

    public List<ItemData> AllComponent = new List<ItemData>();

    [OdinSerialize, NonSerialized]
    public Dictionary<(ItemData, ItemData), ItemData> ItemCombinationTable = new Dictionary<(ItemData, ItemData), ItemData>();

    public ItemData GetRandomItem(bool canGetComplete = false)
    {
        if (canGetComplete)
        {
            return AllItems[Random.Range(0, AllItems.Count)];
        }

        return AllComponent[Random.Range(0, AllComponent.Count)];
    }

    public ItemData GetRandomEmblem()
    {
        return AllEmblems[Random.Range(0, AllEmblems.Count)];
    }

    public int GetIndex(ItemData itemData)
    {
        if (AllItems.Contains(itemData))
        {
            return AllItems.IndexOf(itemData);
        }

        Debug.LogError("Could not find the ItemData");
        return -1;
    }

    public ItemData Get(int index)
    {
        if (index >= 0 && index < AllItems.Count)
        {
            return AllItems[index];
        }

        Debug.LogError("Argument out of range exception");
        return null;
    }

    public ItemData CombineItems(ItemData component1, ItemData component2)
    {
        //Debug.Log("Combining: " + component1.name + " and " + component2.name);
        if (ItemCombinationTable.TryGetValue((component1, component2), out ItemData value))
        {
            return value;
        }

        if (ItemCombinationTable.TryGetValue((component2, component1), out ItemData value2))
        {
            return value2;
        }

        Debug.LogError("Could not find item combination");
        return null;
    }
}
