using Effects;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
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
            CritChance = unit.UnitStats.CritChance.Value,
            CritMultiplier = unit.UnitStats.CritMultiplier.Value,
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
        for (int i = 0; i < battle.AlliedUnits.Count; i++)
        {
            battle.AlliedUnits[i].UnitHealth.AddHealth(health);

            ParticleManager.Instance.LeavesParticle.GetAtPosAndRot<PooledMonoBehaviour>(battle.AlliedUnits[i].transform.position, Quaternion.identity);
        }

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

