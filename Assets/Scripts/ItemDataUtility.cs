using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[InlineEditor]
[CreateAssetMenu(fileName = "Item Data Utility", menuName = "Item/Item Data Utility")]
public class ItemDataUtility : ScriptableObject
{
    public List<ItemData> AllItems = new List<ItemData>();

    public List<ItemData> AllComponent = new List<ItemData>();

    public ItemData GetRandomItem(bool canGetComplete = false)
    {
        if (canGetComplete)
        {
            return AllItems[Random.Range(0, AllItems.Count)];
        }

        return AllComponent[Random.Range(0, AllComponent.Count)];
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
}
