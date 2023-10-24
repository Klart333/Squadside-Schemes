using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitBaseState
{
    protected UnitBaseState(Unit unit)
    {
        Unit = unit;
    }

    public Unit Unit { get; set; }

    public abstract void TakeAction(Battle battle);

    public abstract bool IsPerformingAction();
}


public class UnitMoveState : UnitBaseState
{
    private bool moving = false;

    private float lastTimeStamp = 0;

    public UnitMoveState(Unit unit) : base(unit)
    {
    }

    public override bool IsPerformingAction()
    {
        return moving;
    }

    public override void TakeAction(Battle battle)
    {
        if (Time.time - lastTimeStamp < 0.1f)
        {
            return;
        }
        
        lastTimeStamp = Time.time;

        Tile targetTile = battle.GetEnemyUnits(Unit)[0].CurrentTile;

        List<Tile> path = PathFinding.FindPath(Unit.CurrentTile, targetTile, battle.Board);
        if (path == null || path.Count <= 0)
        {
            return;
        }

        Unit.UnitAnimator.PlayMove();

        Move(path[path.Count - 1]);

        Unit.CurrentTile.CurrentUnit = null;

        Unit.CurrentTile = path[path.Count - 1];
        path[path.Count - 1].CurrentUnit = Unit;
    }

    public async void Move(Tile targetTile)
    {
        moving = true;

        float t = 0;

        Vector3 dir = (targetTile.WorldPosition - Unit.CurrentTile.WorldPosition).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);

        Tile startTile = Unit.CurrentTile;

        startTile.UnitLeaving = true;

        while (t <= 1.0f)
        {
            if (Unit == null)
            {
                return;
            }

            t += Time.deltaTime * Unit.UnitStats.MovementSpeed.Value;

            Unit.transform.position = Vector3.Lerp(startTile.WorldPosition, targetTile.WorldPosition, Mathf.SmoothStep(0.0f, 1.0f, t));

            if (t < 0.3f)
            {
                Unit.transform.rotation = Quaternion.Slerp(Unit.transform.rotation, targetRotation, t);
            }

            await UniTask.Yield();
        }

        startTile.UnitLeaving = false;
        moving = false;
    }
}

public class UnitAttackState : UnitBaseState
{
    private bool attacking = false;

    public UnitAttackState(Unit unit) : base(unit)
    {
    }

    public override bool IsPerformingAction()
    {
        return attacking;
    }

    public override async void TakeAction(Battle battle)
    {
        attacking = true;

        await UniTask.Delay(TimeSpan.FromSeconds(1.0f / Unit.UnitStats.AttackSpeed.Value) * 0.8f);

        if (Unit == null) 
        {
            attacking = false;
            return;
        }

        List<Unit> enemies = battle.GetEnemyUnits(Unit);

        if (enemies == null || enemies.Count == 0)
        {
            attacking = false;
            return;
        }

        Unit.UnitAnimator.PlayAttack();

        DamageInstance damageInstance = new DamageInstance
        {
            UnitSource = Unit,
            UnitTarget = enemies[0],

            AttackDamage = Unit.UnitStats.AttackDamage.Value,
            CritChance = Unit.UnitStats.CritChance.Value,
            CritMultiplier = Unit.UnitStats.CritMultiplier.Value,
            AbilityDamage = 0,
            TrueDamage = 0,
        };

        if (enemies[0].TakeDamage(damageInstance, out DamageInstance damageDone))
        {
            Unit.OnUnitKill();
        }
        else
        {
            RotateTowardTarget(enemies[0].CurrentTile);
        }

        Unit.OnUnitAttack();
        Unit.OnUnitDoneDamage(damageDone);
        Unit.BattleController.LastAttackDone = damageDone;

        await UniTask.Delay(TimeSpan.FromSeconds(1.0f / Unit.UnitStats.AttackSpeed.Value) * 0.2f);

        attacking = false;
    }

    private async void RotateTowardTarget(Tile targetTile)
    {
        Vector3 dir = (targetTile.WorldPosition - Unit.CurrentTile.WorldPosition).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
        float t = 0;

        while (t <= 0.5f)
        {
            if (Unit == null)
            {
                return;
            }

            t += Time.deltaTime * Unit.UnitStats.MovementSpeed.Value;

            Unit.transform.rotation = Quaternion.Slerp(Unit.transform.rotation, targetRotation, t);

            await UniTask.Yield();
        }
    }
}

public class UnitUltimateState : UnitBaseState
{
    private bool attacking = false;

    public UnitUltimateState(Unit unit) : base(unit)
    {
    }

    public override bool IsPerformingAction()
    {
        return attacking;
    }

    public override async void TakeAction(Battle battle)
    {
        attacking = true;
        List<Unit> enemies = battle.GetEnemyUnits(Unit);

        Unit.UnitAnimator.PlayUlt();

        if (!Unit.UnitData.UltimateAttack.Perform(Unit, enemies[0], battle, out DamageInstance damageDone))
        {
            RotateTowardTarget(enemies[0].CurrentTile);
        }

        //Unit.OnUnitAttack();
        if (damageDone != null)
        {
            Unit.OnUnitDoneDamage(damageDone);
            Unit.BattleController.LastAttackDone = damageDone;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(1.0f / Unit.UnitStats.AttackSpeed.Value));

        attacking = false;
    }

    private async void RotateTowardTarget(Tile targetTile)
    {
        Vector3 dir = (targetTile.WorldPosition - Unit.CurrentTile.WorldPosition).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
        float t = 0;

        while (t <= 0.5f)
        {
            if (Unit == null)
            {
                return;
            }

            t += Time.deltaTime * Unit.UnitStats.MovementSpeed.Value;

            Unit.transform.rotation = Quaternion.Slerp(Unit.transform.rotation, targetRotation, t);

            await UniTask.Yield();
        }
    }
}

