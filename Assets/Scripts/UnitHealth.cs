using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    [Title("Health")]
    [SerializeField]
    private UIHealthBar healthBar;

    [SerializeField]
    private UIDamageNumber damageNumber;

    [Title("Particles")]
    [SerializeField]
    private PooledMonoBehaviour hitParticle;

    [SerializeField]
    private PooledMonoBehaviour deathParticle;

    private Unit unit;

    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float HealthPercentage => CurrentHealth / unit.UnitStats.MaxHealth.Value;
    public DamageInstance LastDamageTaken { get; private set; }

    private void OnEnable()
    {
        unit = GetComponent<Unit>();

        unit.OnCombatStart += MaxCurrentHealth;

        InitializeHealth();
    }

    private void OnDisable()
    {
        if (unit != null && unit.IsOwner)
        {
            unit.OnCombatStart -= MaxCurrentHealth;

            if (unit.UnitStats != null)
            {
                unit.UnitStats.MaxHealth.OnValueChanged -= UpdateMaxHealth;
            }
        }
    }

    public void MaxCurrentHealth()
    {
        currentHealth = unit.UnitStats.MaxHealth.Value;
        healthBar.UpdateHealthBar(currentHealth);
    }

    private async void InitializeHealth()
    {
        await UniTask.WaitUntil(() => unit.UnitStats != null);

        unit.UnitStats.MaxHealth.OnValueChanged += UpdateMaxHealth;

        UpdateMaxHealth();
        MaxCurrentHealth();
    }

    public void UpdateMaxHealth()
    {
        healthBar.SetMaxHealth((int)unit.UnitStats.MaxHealth.Value);

        if (unit.PlayerHandler != null && unit.PlayerHandler.BattleSystem != null && !unit.PlayerHandler.BattleSystem.IsInBattle)
        {
            MaxCurrentHealth();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>If the unit died</returns>
    public bool TakeDamage(DamageInstance damageInstance, out DamageInstance damageDone)
    {
        if (!unit.PlayerHandler.BattleSystem.IsInBattle)
        {
            MaxCurrentHealth();
            damageDone = new DamageInstance();
            return false;
        }

        damageDone = EvaluateDamage(damageInstance);
        currentHealth -= damageDone.GetTotal();

        healthBar.UpdateHealthBar(currentHealth);

        if (currentHealth <= 0)
        {
            deathParticle.GetAtPosAndRot<PooledMonoBehaviour>(transform.position + Vector3.up * 1, Quaternion.identity);

            unit.LocalDeath();
            return true;
        }

        hitParticle.GetAtPosAndRot<PooledMonoBehaviour>(transform.position + Vector3.up * 0.5f, Quaternion.identity);

        return false;
    }

    private DamageInstance EvaluateDamage(DamageInstance damageInstance)
    {
        float critMult = damageInstance.CritMultiplier;
        damageInstance.AttackDamage *= critMult;
        damageInstance.AbilityDamage *= critMult;
        damageInstance.TrueDamage *= critMult;

        float trueDamage = damageInstance.TrueDamage;
        float attackDamage = damageInstance.AttackDamage * (1.0f - (unit.UnitStats.Armor.Value / (100.0f + unit.UnitStats.Armor.Value)));
        float abilityDamage = damageInstance.AbilityDamage * (1.0f - (unit.UnitStats.MagicResist.Value / (100.0f + unit.UnitStats.MagicResist.Value)));

        damageInstance.TrueDamage = trueDamage;
        damageInstance.AttackDamage = attackDamage;
        damageInstance.AbilityDamage = abilityDamage;

        LastDamageTaken = damageInstance;

        // Spawn damage numbers
        HandleDamageNumbers(damageInstance, critMult > 1);

        return damageInstance;
    }

    private void HandleDamageNumbers(DamageInstance damageInstance, bool isCrit)
    {
        if (unit.IsEnemyUnit)
        {
            UIDamageNumber dmgNumb = damageNumber.GetAtPosAndRot<UIDamageNumber>(unit.transform.position + Vector3.up, Quaternion.identity);
            dmgNumb.Setup(damageInstance, isCrit);
        }
    }

    public void AddHealth(float amount)
    {
        if (!unit.PlayerHandler.BattleSystem.IsInBattle)
        {
            MaxCurrentHealth();
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, unit.UnitStats.MaxHealth.Value);

        healthBar.UpdateHealthBar(currentHealth);
    }
}

public class DamageInstance
{
    public Unit UnitSource;
    public Unit UnitTarget;

    public float AttackDamage;
    public float AbilityDamage;
    public float TrueDamage;
    public float CritMultiplier;

    private HashSet<int> specialEffectSet;

    public HashSet<int> SpecialEffectSet
    {
        get
        {
            if (specialEffectSet == null)
            {
                specialEffectSet = new HashSet<int>();
            }

            return specialEffectSet;
        }
        set
        {
            specialEffectSet = value;
        }
    }

    /// <summary>
    /// Does not apply crit
    /// </summary>
    /// <returns></returns>
    public float GetTotal()
    {
        return AttackDamage + AbilityDamage + TrueDamage;
    }
}
