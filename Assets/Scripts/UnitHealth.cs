using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    [Title("Health")]
    [SerializeField]
    private UIHealthBar healthBar;

    private Unit unit;

    private float currentHealth;

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
    }

    public bool TakeDamage(float damage)
    {
        if (!unit.PlayerHandler.BattleSystem.IsInBattle)
        {
            MaxCurrentHealth();
            return false;
        }

        currentHealth -= damage;

        healthBar.UpdateHealthBar(currentHealth);

        if (currentHealth <= 0)
        {
            unit.LocalDeath();
            return true;
        }

        return false;
    }

    public void AddHealth(float amount)
    {
        if (!unit.PlayerHandler.BattleSystem.IsInBattle)
        {
            MaxCurrentHealth();
            return;
        }

        currentHealth += amount;

        healthBar.UpdateHealthBar(currentHealth);
    }
}
