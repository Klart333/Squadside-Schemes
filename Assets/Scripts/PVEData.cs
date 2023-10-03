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
    public List<List<UnitNetworkData>> MobData = new List<List<UnitNetworkData>>();
}
