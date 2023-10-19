using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PVE Data", menuName = "PVE/Mob Data")]
public class PVEData : SerializedScriptableObject
{
    [NonSerialized]
    [OdinSerialize]
    public List<List<MobNetworkData>> MobData = new List<List<MobNetworkData>>();

    public List<UnitNetworkData> GetMobData(int index)
    {
        index = Mathf.Clamp(index, 0, MobData.Count - 1);

        List<UnitNetworkData> mobs = new List<UnitNetworkData>();
        for (int i = 0; i < MobData[index].Count; i++)
        {
            UnitNetworkData unitNetworkData = new UnitNetworkData
            {
                StarLevel = MobData[index][i].StarLevel,
                UnitDataIndex = GameManager.Instance.UnitDataUtility.GetIndex(MobData[index][i].UnitData),

                TileIndexX = MobData[index][i].TileIndex.x,
                TileIndexY = MobData[index][i].TileIndex.y,

                ItemIndex0 = MobData[index][i].ItemSlots[0] == null ? -1 : GameManager.Instance.ItemDataUtility.GetIndex(MobData[index][i].ItemSlots[0]),
                ItemIndex1 = MobData[index][i].ItemSlots[1] == null ? -1 : GameManager.Instance.ItemDataUtility.GetIndex(MobData[index][i].ItemSlots[1]),
                ItemIndex2 = MobData[index][i].ItemSlots[2] == null ? -1 : GameManager.Instance.ItemDataUtility.GetIndex(MobData[index][i].ItemSlots[2])
            };

            mobs.Add(unitNetworkData);
        }

        return mobs;
    }
}

public struct MobNetworkData
{
    [Title("Unit")]
    public UnitData UnitData;
    public int StarLevel;

    [Title("Items")]
    public ItemData[] ItemSlots;

    [Title("Position")]
    public Vector2Int TileIndex;
}
