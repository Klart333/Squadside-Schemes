using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Effects
{
    public interface IEffect
    {
        public void Perform(Unit unit);
        public void Revert(Unit unit);
    }

    #region Gain Max Health

    [InlineEditor]
    public class GainMaxHealthEffect : IEffect
    {
        public int HealthToGain = 10;

        private Dictionary<Unit, Modifier> ModifierDictionary = new Dictionary<Unit, Modifier>();

        public void Perform(Unit unit)
        {
            if (ModifierDictionary == null)
            {
                ModifierDictionary = new Dictionary<Unit, Modifier>();
            }

            if (!ModifierDictionary.ContainsKey(unit))
            {
                ModifierDictionary.Add(unit, new Modifier { Type = Modifier.ModifierType.Additive, Value = HealthToGain });
                unit.UnitStats.MaxHealth.AddModifier(ModifierDictionary[unit]);
            }
            else
            {
                ModifierDictionary[unit].Value += HealthToGain;
            }

            unit.UnitHealth.AddHealth(HealthToGain);
            unit.UnitHealth.UpdateMaxHealth();
        }

        public void Revert(Unit unit)
        {
            if (ModifierDictionary == null || !ModifierDictionary.ContainsKey(unit))
            {
                return;
            }

            unit.UnitStats.MaxHealth.RemoveModifier(ModifierDictionary[unit]);
            unit.UnitHealth.MaxCurrentHealth();

            ModifierDictionary.Remove(unit);
        }
    }

    #endregion

    #region Gain Attack Speed

    public class GainAttackSpeedEffect : IEffect
    {
        public float AttackSpeedGain = 1.1f;

        private Dictionary<Unit, Modifier> ModifierDictionary;

        public void Perform(Unit unit)
        {
            if (ModifierDictionary == null)
            {
                ModifierDictionary = new Dictionary<Unit, Modifier>();
            }

            if (!ModifierDictionary.ContainsKey(unit))
            {
                ModifierDictionary.Add(unit, new Modifier { Type = Modifier.ModifierType.Multiplicative, Value = AttackSpeedGain });
                unit.UnitStats.AttackSpeed.AddModifier(ModifierDictionary[unit]);
            }
            else
            {
                ModifierDictionary[unit].Value *= AttackSpeedGain;
            }
        }

        public void Revert(Unit unit)
        {
            if (ModifierDictionary == null || !ModifierDictionary.ContainsKey(unit))
            {
                return;
            }

            unit.UnitStats.AttackSpeed.RemoveModifier(ModifierDictionary[unit]);

            ModifierDictionary.Remove(unit);
        }
    }

    #endregion

    #region Gain Attack Damage

    public class GainAttackDamageEffect : IEffect
    {
        public float AttackDamage = 20f;

        private Dictionary<Unit, Modifier> ModifierDictionary;

        public void Perform(Unit unit)
        {
            if (ModifierDictionary == null)
            {
                ModifierDictionary = new Dictionary<Unit, Modifier>();
            }

            if (!ModifierDictionary.ContainsKey(unit))
            {
                ModifierDictionary.Add(unit, new Modifier { Type = Modifier.ModifierType.Additive, Value = AttackDamage });
                unit.UnitStats.AttackDamage.AddModifier(ModifierDictionary[unit]);
            }
            else
            {
                ModifierDictionary[unit].Value += AttackDamage;
            }
        }

        public void Revert(Unit unit)
        {
            if (ModifierDictionary == null || !ModifierDictionary.ContainsKey(unit))
            {
                return;
            }

            unit.UnitStats.AttackDamage.RemoveModifier(ModifierDictionary[unit]);

            ModifierDictionary.Remove(unit);
        }
    }

    #endregion

}

