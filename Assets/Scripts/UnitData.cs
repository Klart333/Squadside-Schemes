using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.Netcode;
using UnityEngine;

[InlineEditor]
[CreateAssetMenu(fileName = "New Data", menuName = "Unit/Unit Data")]
public class UnitData : SerializedScriptableObject
{
    [Title("Unit")]
    public string Name;

    public int Cost;

    public Sprite Portrait;

    public Unit UnitPrefab;

    [Title("Traits")]
    public Trait[] Traits;

    [Title("Traits")]
    [OdinSerialize]
    public IUltimateAttack UltimateAttack;

    [Title("Combat", "Attack")]
    public float AttackDamage = 50;
    public float AttackSpeed = 0.7f;
    public int AttackRange = 1;

    [Title("Combat", "Ability")]
    public float AbilityPower = 100;
    public float Mana = 10;
    public float MaxMana = 100;

    [Title("Combat", "Crit")]
    public float CritChance = 0.25f;
    public float CritMultiplier = 2.0f;

    [Title("Defense")]
    public float Armor = 30;
    public float MagicResist = 25;

    [Title("Health")]
    public int BaseHealth = 500;
    public float Omnivamp = 0;

    [Title("Movement")]
    public float MovementSpeed = 1;
}

public struct UnitNetworkData : INetworkSerializable, System.IEquatable<UnitNetworkData>
{
    [Title("Unit")]
    public int UnitDataIndex;
    public int StarLevel;

    [Title("Items")]
    public int ItemIndex0;
    public int ItemIndex1;
    public int ItemIndex2;

    [Title("Position")]
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

