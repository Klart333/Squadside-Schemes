using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

[InlineEditor]
[CreateAssetMenu(fileName = "New Data", menuName = "Unit/Unit Data")]
public class UnitData : ScriptableObject
{
    [Title("Unit")]
    public string Name;

    public int Cost;

    public Sprite Portrait;

    public Unit UnitPrefab;

    [Title("Unit")]
    public Trait[] Traits;

    [Title("Combat")]
    public float AttackDamage = 50;
    public float AttackSpeed = 0.7f;
    public float AbilityPower = 100;

    public int AttackRange = 1;

    [Title("Health")]
    public int BaseHealth = 500;

    [Title("Movement")]
    public float MovementSpeed = 1;
}

public struct UnitNetworkData : INetworkSerializable, System.IEquatable<UnitNetworkData>
{
    public int UnitDataIndex;
    public int StarLevel;
    public int ItemIndex0;
    public int ItemIndex1;
    public int ItemIndex2;
    public int TileIndexX;
    public int TileIndexY;

    public bool Equals(UnitNetworkData other)
    {
        return other.UnitDataIndex == this.UnitDataIndex
            && other.StarLevel == this.UnitDataIndex
            && other.TileIndexX == this.TileIndexX
            && other.TileIndexY == this.TileIndexY;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref UnitDataIndex);
        serializer.SerializeValue(ref StarLevel);

        serializer.SerializeValue(ref ItemIndex0);
        serializer.SerializeValue(ref ItemIndex1);
        serializer.SerializeValue(ref ItemIndex2);

        serializer.SerializeValue(ref TileIndexX);
        serializer.SerializeValue(ref TileIndexY);
    }
}

