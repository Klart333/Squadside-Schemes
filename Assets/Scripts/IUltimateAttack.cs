using Effects;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

public interface IUltimateAttack
{
    public bool Perform(Unit unit, Unit target, Battle battle, out DamageInstance damageDone);
}

public class BiteAttack : IUltimateAttack
{
    [TitleGroup("Attack")]
    public bool UseAttackDamage;
    [TitleGroup("Attack"), ShowIf(nameof(UseAttackDamage))]
    public float AttackDamage;
    [TitleGroup("Attack"), ShowIf(nameof(UseAttackDamage))]
    public float AttackScaling = 1.0f;

    [TitleGroup("Ability")]
    public bool UseAbilityDamage;
    [TitleGroup("Ability"), ShowIf(nameof(UseAbilityDamage))]
    public float AbilityDamage;
    [TitleGroup("Ability"), ShowIf(nameof(UseAbilityDamage))]
    public float AbilityScaling = 1.0f;

    public bool Perform(Unit unit, Unit target, Battle battle, out DamageInstance damageDone)
    {
        float attackDamage = AttackDamage + AttackScaling * unit.UnitStats.AttackDamage.Value;
        float abilityDamage = AbilityDamage + AbilityScaling * unit.UnitStats.AbilityPower.Value;

        DamageInstance damageInstance = new DamageInstance
        {
            UnitSource = unit,
            UnitTarget = target,

            AttackDamage = attackDamage,
            AbilityDamage = abilityDamage,
            CritMultiplier = unit.UnitStats.GetCritMultiplier(),
            TrueDamage = 0
        };

        if (target.TakeDamage(damageInstance, out damageDone))
        {
            unit.OnUnitKill();
            return true;
        }

        return false;
    }
}

public class TeamHeal : IUltimateAttack
{
    [Title("Heal")]
    public float Healing = 200;
    public float AbilityScaling = 1.0f;

    public bool Perform(Unit unit, Unit target, Battle battle, out DamageInstance damageDone)
    {
        damageDone = null;

        float health = Healing + AbilityScaling * unit.UnitStats.AbilityPower.Value;
        List<Unit> targets = unit.IsEnemyUnit ? battle.EnemyUnits : battle.AlliedUnits;

        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].UnitHealth.AddHealth(health);

            ParticleManager.Instance.LeavesParticle.GetAtPosAndRot<PooledMonoBehaviour>(targets[i].transform.position, Quaternion.identity);
        }

        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.Crunch);

        return false;
    }
}

public class UltimateEffect : IUltimateAttack
{
    [OdinSerialize]
    [InfoBox("Please be mindful with the effects used, does not get reverted")]
    public IEffect IEffect;

    public bool Perform(Unit unit, Unit target, Battle battle, out DamageInstance damageDone)
    {
        damageDone = null;
        IEffect.Perform(unit);

        return false;
    }
}

