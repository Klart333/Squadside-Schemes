using System.Collections.Generic;
using UnityEngine;

public class UnitBattleController
{
    public Unit Unit { get; set; }

    public UnitBaseState CurrentState;

    public UnitMoveState UnitMoveState;
    public UnitAttackState UnitAttackState;
    public UnitUltimateState UnitUltimateState;

    private DamageInstance lastAttackDone;
    public DamageInstance LastAttackDone
    {
        get
        {
            return lastAttackDone;
        }
        set
        {
            lastAttackDone = value;
            Unit.LastDamageDone = value;
        }
    }

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

        if (GetDistance(enemyUnits[0].CurrentTile) > Unit.UnitStats.AttackRange.Value)
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

    public void ActivateOvertime()
    {
        Unit.UnitStats.AttackSpeed.AddModifier(new Modifier { Type = Modifier.ModifierType.Additive, Value = 5 - Unit.UnitStats.AttackSpeed.Value });
        Unit.UnitStats.AbilityPower.AddModifier(new Modifier { Type = Modifier.ModifierType.Multiplicative, Value = 3 });

        // The only units to be overtimed should only be temporary, so there's no need to remove the effect hopefully
    }
}
