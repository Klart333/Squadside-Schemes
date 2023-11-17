using UnityEngine;

public class Tile
{
    public Vector3 WorldPosition;
    public Vector2Int Index;
    public Unit CurrentUnit;

    public bool Walkable => CurrentUnit == null && !UnitLeaving;
    public bool UnitLeaving = false;

    public bool IsBench => Index.y <= -1;
    public int LongIndex => Index.x + Index.y * BoardSystem.BoardX;
}
