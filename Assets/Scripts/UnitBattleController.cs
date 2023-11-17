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
        SortEnemiesByDistance(enemyUnits);

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

    public void SortEnemiesByDistance(List<Unit> enemies)
    {
        InsertionSortEnemies(enemies, enemies.Count);
    }

    public void InsertionSortEnemies(List<Unit> list, int length)
    {
        for (int i = 1; i < length; i++)
        {
            Unit unit = list[i];
            int key = GetDistance(unit.CurrentTile);

            for (int j = i - 1; j >= 0;)
            {
                int dist = GetDistance(list[j].CurrentTile);
                if (key < dist)
                {
                    list[j + 1] = list[j];
                    j--;
                    list[j + 1] = unit;
                }
                else if (key == dist) // If they're the same length I gotta sort from the index on the board, but the enemies will be mirrored, so i gotta reverse if its an enemy
                {
                    if (unit.IsEnemyUnit)
                    {
                        if (unit.CurrentTile.LongIndex > list[j].CurrentTile.LongIndex)
                        {
                            list[j + 1] = list[j];
                            j--;
                            list[j + 1] = unit;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (unit.CurrentTile.LongIndex < list[j].CurrentTile.LongIndex)
                        {
                            list[j + 1] = list[j];
                            j--;
                            list[j + 1] = unit;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    public int GetDistance(Tile tile)
    {
        if (tile == null)
        {
            Debug.LogError("Dont send null tiles");
            return 999;
        }

        return Mathf.RoundToInt(Vector3.Distance(tile.WorldPosition, Unit.CurrentTile.WorldPosition) / BoardSystem.TileScale);
    }

    public void ActivateOvertime()
    {
        Unit.UnitStats.AttackSpeed.AddModifier(new Modifier { Type = Modifier.ModifierType.Additive, Value = 5 - Unit.UnitStats.AttackSpeed.Value });
        Unit.UnitStats.AbilityPower.AddModifier(new Modifier { Type = Modifier.ModifierType.Multiplicative, Value = 3 });

        // The only units to be overtimed should only be temporary, so there's no need to remove the effect hopefully
    }

    public List<Unit> QuickSortEnemies(List<Unit> list, int leftIndex, int rightIndex)
    {
        int i = leftIndex;
        int j = rightIndex;
        int pivot = GetDistance(list[i].CurrentTile);

        while (i <= j)
        {
            while (GetDistance(list[i].CurrentTile) < pivot)
            {
                i++;
            }

            while (GetDistance(list[i].CurrentTile) > pivot)
            {
                j--;
            }

            if (i <= j) // If we're not done
            {
                Unit temp = list[i];
                list[i] = list[j];
                list[j] = temp;
                i++;
                j--;
            }
        }

        if (leftIndex < j)
        {
            QuickSortEnemies(list, leftIndex, j);
        }

        if (i < rightIndex)
        {
            QuickSortEnemies(list, i, rightIndex);
        }

        return list;
    }

}
