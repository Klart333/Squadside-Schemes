using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class SerializationExtensions
{
    public static void ReadValueSafe(this FastBufferReader reader, out UnitData data)
    {
        reader.ReadValueSafe(out int index);
        if (index != -1)
        {
            data = GameManager.Instance.UnitDataUtility.Get(index);
        }
        else
        {
            data = null;
        }

        //Debug.Log("ReadValueSafe " + index + ", " + data);
    }

    public static void WriteValueSafe(this FastBufferWriter write, in UnitData data)
    {
        if (data != null)
        {
            write.WriteValueSafe(GameManager.Instance.UnitDataUtility.GetIndex(data));
        }
        else
        {
            write.WriteValueSafe(-1);
        }

        //Debug.Log("WriteValueSafe " + GameManager.Instance.UnitDataUtility.GetIndex(data) + ", " + data);
    }

    public static void DuplicateValueSafe(in UnitData value, ref UnitData duplicatedValue)
    {
        if (value != null)
        {
            duplicatedValue = ScriptableObject.Instantiate(value);
        }

        //Debug.Log("Tried to duplicate but im not sure its gonna work");
    }
}
