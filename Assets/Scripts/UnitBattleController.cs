using System.Collections.Generic;
using UnityEngine;

public class UnitBattleController
{
    public Unit Unit { get; set; }

    public UnitBaseState CurrentState;

    public UnitMoveState UnitMoveState;
    public UnitAttackState UnitAttackState;
    public UnitUltimateState UnitUltimateState;

    public void Update(Battle battle)
    {
        if (CurrentState == null)
        {
            CurrentState = UnitMoveState;
        }

        if (CurrentState.IsPerformingAction())
        {
            return;
        }

        CurrentState = EvaluateState(battle.GetEnemyUnits(Unit));

        CurrentState.TakeAction(battle);
    }

    private UnitBaseState EvaluateState(List<Unit> enemyUnits)
    {
        SortEnemiesByDistance(ref enemyUnits);

        if (GetDistance(enemyUnits[0].CurrentTile) > Unit.UnitData.AttackRange)
        {
            return UnitMoveState;
        }

        if (Unit.UnitStats.Mana.Value >= Unit.UnitStats.MaxMana.Value)
        {
            Unit.UnitStats.Mana.RemoveAllModifiers();
            return UnitUltimateState;
        }

        return UnitAttackState;
    }

    public void SortEnemiesByDistance(ref List<Unit> enemies)
    {
        enemies.Sort((x, y) => GetDistance(x.CurrentTile).CompareTo(GetDistance(y.CurrentTile)));
    }

    public int GetDistance(Tile tile)
    {
        if (tile == null)
        {
            Debug.LogError("Dont send null tiles");
        }

        return Mathf.RoundToInt(Vector3.Distance(tile.WorldPosition, Unit.CurrentTile.WorldPosition) / BoardSystem.TileScale);
    }
}
