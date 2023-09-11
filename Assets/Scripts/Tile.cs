using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public Vector3 WorldPosition;
    public Vector2Int Index;
    public Unit CurrentUnit;

    public bool Walkable => CurrentUnit == null && !UnitLeaving;
    public bool UnitLeaving = false;
}
