using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Effects
{
    public interface IEffect
    {
        public void Perform(Unit unit);
        public void Revert(Unit unit);

        public float ModifierValue { get; set; }
    }

    #region Increase Stat

    public class IncreaseStatEffect : IEffect
    {
        [TitleGroup("Modifier")]
        [OdinSerialize]
        public float ModifierValue { get; set; } = 20f;

        [TitleGroup("Modifier")]
        public Modifier.ModifierType ModifierType;

        [TitleGroup("Modifier")]
        public StatType StatType;

        [TitleGroup("Options")]
        public bool CanIncrease = true;

        private Dictionary<Unit, Modifier> ModifierDictionary;

        public void Perform(Unit unit)
        {
            if (ModifierDictionary == null)
            {
                ModifierDictionary = new Dictionary<Unit, Modifier>();
            }

            if (!ModifierDictionary.ContainsKey(unit))
            {
                ModifierDictionary.Add(unit, new Modifier
                {
                    Type = ModifierType,
                    Value = ModifierValue
                });

                unit.UnitStats.ModifyStat(StatType, ModifierDictionary[unit]);
            }
            else if (CanIncrease)
            {
                ModifierDictionary[unit].Value += ModifierValue;
            }

            if (StatType == StatType.MaxHealth)
            {
                ParticleManager.Instance.LeavesParticle.GetAtPosAndRot<PooledMonoBehaviour>(unit.transform.position, Quaternion.identity);
            }
        }

        public void Revert(Unit unit)
        {
            if (ModifierDictionary == null || !ModifierDictionary.ContainsKey(unit))
            {
                return;
            }

            unit.UnitStats.RevertModifiedStat(StatType, ModifierDictionary[unit]);

            ModifierDictionary.Remove(unit);
        }
    }

    #endregion

    #region Timed Stat Increase

    public class TimedStatIncreaseEffect : IEffect
    {
        [TitleGroup("Modifier")]
        [OdinSerialize]
        public float ModifierValue { get; set; } = 20f;

        [TitleGroup("Modifier")]
        public Modifier.ModifierType ModifierType;

        [TitleGroup("Modifier")]
        public StatType StatType;

        [TitleGroup("Modifier")]
        public float Time = 3;

        [TitleGroup("Modifier")]
        [ReadOnly]
        public bool CanIncrease = false;

        private Dictionary<Unit, Modifier> ModifierDictionary;

        public async void Perform(Unit unit)
        {
            if (ModifierDictionary == null)
            {
                ModifierDictionary = new Dictionary<Unit, Modifier>();
            }

            if (ModifierDictionary.ContainsKey(unit))
            {
                return;
            }

            ModifierDictionary.Add(unit, new Modifier
            {
                Type = ModifierType,
                Value = ModifierValue
            });

            unit.UnitStats.ModifyStat(StatType, ModifierDictionary[unit]);

            await UniTask.Delay(TimeSpan.FromSeconds(Time));

            if (unit == null)
            {
                if (ModifierDictionary.ContainsKey(unit))
                {
                    ModifierDictionary.Remove(unit);
                }

                return;
            }

            Revert(unit);
        }

        public void Revert(Unit unit)
        {
            if (ModifierDictionary == null || !ModifierDictionary.ContainsKey(unit))
            {
                return;
            }

            unit.UnitStats.RevertModifiedStat(StatType, ModifierDictionary[unit]);

            ModifierDictionary.Remove(unit);
        }
    }

    #endregion

    #region Temporary Increase Stat

    public class TemporaryIncreaseStatEffect : IEffect
    {
        [TitleGroup("Modifier")]
        [OdinSerialize]
        public float ModifierValue { get; set; } = 20f;

        [TitleGroup("Modifier")]
        public Modifier.ModifierType ModifierType;

        [TitleGroup("Modifier")]
        public StatType StatType;

        [TitleGroup("Modifier")]
        public float ChanceToTrigger = 0.2f;

        private HashSet<Unit> unitsAttacking = new HashSet<Unit>();

        public void Perform(Unit unit)
        {
            if (unitsAttacking == null) unitsAttacking = new HashSet<Unit>();

            if (unitsAttacking.Contains(unit)) return;

            unit.UnitStats.ModifyStat(StatType, new Modifier
            {
                Type = ModifierType,
                Value = ModifierValue
            });

            Action RevertAfterAttack = null;
            RevertAfterAttack = () =>
            {
                Revert(unit);
                unit.OnAttack -= RevertAfterAttack;
            };

            unit.OnAttack += RevertAfterAttack;
            unitsAttacking.Add(unit);
        }

        public async void Revert(Unit unit)
        {
            if (unitsAttacking == null)
            {
                return;
            }

            unit.UnitStats.RevertModifiedStat(StatType, new Modifier
            {
                Type = ModifierType,
                Value = ModifierValue
            });

            await UniTask.NextFrame();
            unitsAttacking.Remove(unit);
        }
    }

    #endregion

    #region Stacking Effect

    public class StackingEffectEffect : IEffect
    {
        [TitleGroup("Stat Increase")]
        [OdinSerialize]
        public float ModifierValue { get; set; } = 1f;

        [TitleGroup("Stat Increase")]
        public bool ShouldMultiply = false;

        [Title("Effect")]
        [OdinSerialize]
        public IEffect EffectToStack;

        private Dictionary<Unit, float> MultiplierDictionary;

        public void Perform(Unit unit)
        {
            if (MultiplierDictionary == null)
            {
                MultiplierDictionary = new Dictionary<Unit, float>();
            }

            if (!MultiplierDictionary.ContainsKey(unit))
            {
                if (ShouldMultiply)
                {
                    MultiplierDictionary.Add(unit, EffectToStack.ModifierValue);
                }
                else
                {
                    MultiplierDictionary.Add(unit, ModifierValue);
                }
            }
            else
            {
                if (ShouldMultiply)
                {
                    MultiplierDictionary[unit] *= this.ModifierValue;
                }
                else
                {
                    MultiplierDictionary[unit] += this.ModifierValue;
                }
            }

            float value = EffectToStack.ModifierValue;

            EffectToStack.ModifierValue = MultiplierDictionary[unit];
            EffectToStack.Perform(unit);

            EffectToStack.ModifierValue = value;
        }

        public void Revert(Unit unit)
        {
            if (MultiplierDictionary == null || !MultiplierDictionary.ContainsKey(unit))
            {
                return;
            }

            float value = EffectToStack.ModifierValue;

            EffectToStack.ModifierValue = MultiplierDictionary[unit];
            EffectToStack.Revert(unit);

            EffectToStack.ModifierValue = value;

            MultiplierDictionary.Remove(unit);
        }
    }

    #endregion

    #region Reflect Damage Taken

    public class ReflectDamageTakenEffect : IEffect
    {
        [TitleGroup("Percent Damage Reflected")]
        [OdinSerialize]
        public float ModifierValue { get; set; }

        [TitleGroup("Percent Damage Reflected")]
        public float TimePeriod = 0;

        private const float tickRate = 0.2f;

        private const int EffectKey = 100;

        public async void Perform(Unit unit)
        {
            DamageInstance damageToReflect = unit.UnitHealth.LastDamageTaken;

            if (damageToReflect == null || damageToReflect.SpecialEffectSet.Contains(EffectKey) || damageToReflect.UnitTarget == unit)
            {
                return;
            }

            int ticks = Mathf.FloorToInt(Mathf.Max(1, TimePeriod / tickRate));
            float damage = (damageToReflect.GetTotal() * ModifierValue) / ticks;

            DamageInstance reflectInstance = new DamageInstance
            {
                UnitSource = unit,
                UnitTarget = damageToReflect.UnitTarget,

                AbilityDamage = damage,
                CritMultiplier = unit.UnitStats.GetCritMultiplier(),
            };

            reflectInstance.SpecialEffectSet.Add(EffectKey);
            reflectInstance.SpecialEffectSet.Add(200); // Make un-splashable

            for (int i = 0; i < ticks; i++)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(tickRate));

                if (reflectInstance.UnitTarget == null) return;

                ParticleManager.Instance.PoisonParticle.GetAtPosAndRot<PooledMonoBehaviour>(reflectInstance.UnitTarget.transform.position, Quaternion.identity);
                AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.ExtinguishFire);

                reflectInstance.UnitTarget.TakeDamage(reflectInstance, out DamageInstance damageDone);
                if (!damageDone.SpecialEffectSet.Contains(EffectKey))
                {
                    damageDone.SpecialEffectSet.Add(EffectKey);
                    damageDone.SpecialEffectSet.Add(200); // Make un-splashable
                }

                unit.OnUnitDoneDamage(damageDone);
            }
        }

        public void Revert(Unit unit)
        {
            // Nothing to revert
        }
    }

    #endregion

    #region Damage Over Time On Damage

    public class DamageOverTimeOnDamageEffect : IEffect
    {
        [TitleGroup("Percent Damage DOT'd")]
        [OdinSerialize]
        public float ModifierValue { get; set; }

        [TitleGroup("Percent Damage DOT'd")]
        public float TimePeriod = 0;

        private const float tickRate = 0.2f;
        private const int EffectKey = 150;

        public async void Perform(Unit unit)
        {
            DamageInstance damageToDOT = unit.LastDamageDone;

            if (damageToDOT == null || damageToDOT.AbilityDamage <= 1 || damageToDOT.SpecialEffectSet.Contains(EffectKey))
            {
                return;
            }

            int ticks = Mathf.FloorToInt(Mathf.Max(1, TimePeriod / tickRate));
            float totalDamage = (damageToDOT.AbilityDamage + damageToDOT.TrueDamage) * ModifierValue;
            float damage = totalDamage / ticks;
            //Debug.Log(tickRate + ", Triggering DamageOverTime with " + damageToDOT.AbilityDamage + ", that is first multiplied with " + ModifierValue + " resulting in " + totalDamage + ", then that is divided by " + ticks + " finally resulting in " + damage); ;

            DamageInstance dotInstance = new DamageInstance
            {
                UnitSource = unit,
                UnitTarget = damageToDOT.UnitTarget,

                AbilityDamage = damage,
                CritMultiplier = unit.UnitStats.GetCritMultiplier(),
                SpecialEffectSet = damageToDOT.SpecialEffectSet,
            };

            dotInstance.SpecialEffectSet.Add(EffectKey);
            dotInstance.SpecialEffectSet.Add(200); // Make un-splashable

            for (int i = 0; i < ticks; i++)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(tickRate));

                if (dotInstance.UnitTarget == null) return;

                dotInstance.UnitTarget.TakeDamage(dotInstance, out DamageInstance damageDone);
                ParticleManager.Instance.PoisonParticle.GetAtPosAndRot<PooledMonoBehaviour>(dotInstance.UnitTarget.transform.position, Quaternion.identity);
                AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.ExtinguishFire);

                if (!damageDone.SpecialEffectSet.Contains(EffectKey))
                {
                    damageDone.SpecialEffectSet.Add(EffectKey);
                    damageDone.SpecialEffectSet.Add(200); // Make un-splashable
                }
                unit.OnUnitDoneDamage(damageDone);
            }
        }

        public void Revert(Unit unit)
        {
            // Nothing to revert
        }
    }

    #endregion

    #region Splash Damage

    public class SplashDamageEffect : IEffect
    {
        [TitleGroup("Splash Damage Percent")]
        [OdinSerialize]
        public float ModifierValue { get; set; }

        [TitleGroup("Splash Damage Percent")]
        public bool CountAP = true;

        [TitleGroup("Splash Damage Percent")]
        public bool CountAD = false;

        [TitleGroup("Splash Damage Percent")]
        public bool CountTrueDamage = true;

        private const int EffectKey = 200;

        public void Perform(Unit unit)
        {
            DamageInstance damageToSplash = unit.LastDamageDone;

            if (damageToSplash == null || damageToSplash.UnitTarget == null || damageToSplash.SpecialEffectSet.Contains(EffectKey))
            {
                return;
            }

            float damage = 0;
            if (CountAP) damage += damageToSplash.AbilityDamage * ModifierValue;
            if (CountAD) damage += damageToSplash.AttackDamage * ModifierValue;
            if (CountTrueDamage) damage += damageToSplash.TrueDamage * ModifierValue;

            if (damage <= 0)
            {
                return;
            }

            DamageInstance splashInstance = new DamageInstance
            {
                UnitSource = unit,
                UnitTarget = damageToSplash.UnitTarget,

                AbilityDamage = damage,
                CritMultiplier = unit.UnitStats.GetCritMultiplier(),
                SpecialEffectSet = damageToSplash.SpecialEffectSet,
            };

            splashInstance.SpecialEffectSet.Add(EffectKey);
            //splashInstance.SpecialEffectSet.Add(150); // Make un-marriable

            List<Unit> splashableUnits = unit.PlayerHandler.BoardSystem.GetSurroundingUnits(damageToSplash.UnitTarget);

            Debug.Log("Splashing " + splashableUnits.Count + " units");
            AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.Splash);
            for (int i = 0; i < splashableUnits.Count; i++)
            {
                if (splashableUnits[i] == unit)
                {
                    continue;
                }

                splashInstance.UnitTarget = splashableUnits[i];
                splashInstance.UnitTarget.TakeDamage(splashInstance, out DamageInstance damageDone);
                ParticleManager.Instance.SplashParticle.GetAtPosAndRot<PooledMonoBehaviour>(splashInstance.UnitTarget.transform.position, Quaternion.identity);

                if (!damageDone.SpecialEffectSet.Contains(EffectKey))
                {
                    damageDone.SpecialEffectSet.Add(EffectKey);
                    //damageDone.SpecialEffectSet.Add(150);
                }

                //Debug.Log("Splashing " + damageDone.GetTotal() + " damage to " + splashInstance.UnitTarget);

                unit.OnUnitDoneDamage(damageDone);
            }
        }

        public void Revert(Unit unit)
        {
            // Nothing to revert
        }
    }

    #endregion

    #region Give Item

    public class GiveItemEffect : IEffect
    {
        [TitleGroup("Item Amount")]
        [OdinSerialize]
        public float ModifierValue { get; set; }

        [TitleGroup("Item Amount")]
        public bool AllUnits = false;

        [TitleGroup("Item Amount")]
        [HideIf(nameof(AllUnits))]
        public Trait RestrictToTrait;

        private Dictionary<Unit, List<ItemData>> ItemDictionary;

        public void Perform(Unit unit)
        {
            if (!unit.PlayerHandler.BattleSystem.IsInBattle)
            {
                return;
            }

            if (!AllUnits)
            {
                int matchingTraitIndex = GameManager.Instance.TraitUtility.GetIndex(RestrictToTrait);

                if (!unit.IsStrongest(matchingTraitIndex))
                {
                    return;
                }
            }

            if (ItemDictionary == null)
            {
                ItemDictionary = new Dictionary<Unit, List<ItemData>>();
            }

            if (!ItemDictionary.ContainsKey(unit))
            {
                ItemDictionary.Add(unit, new List<ItemData>());
            }

            for (int i = 0; i < ModifierValue; i++)
            {
                ItemData randomItemData = GameManager.Instance.ItemDataUtility.GetRandomItem(true);
                if (!unit.ApplyItem(randomItemData))
                {
                    break;
                }

                ParticleManager.Instance.CircleParticle.GetAtPosAndRot<PooledMonoBehaviour>(unit.transform.position, Quaternion.identity);
                ItemDictionary[unit].Add(randomItemData);
            }
        }

        public void Revert(Unit unit)
        {
            if (ItemDictionary == null)
            {
                return;
            }

            if (ItemDictionary.ContainsKey(unit))
            {
                for (int i = 0; i < ItemDictionary[unit].Count; i++)
                {
                    unit.RemoveItem(ItemDictionary[unit][i]);
                }

                ItemDictionary.Remove(unit);
            }
        }
    }

    #endregion

    #region Stim Bonus 

    public class StimBonusEffect : IEffect
    {
        [TitleGroup("Stim Distance")]
        [OdinSerialize]
        public float ModifierValue { get; set; }

        [TitleGroup("Stat Increase")]
        [OdinSerialize]
        public IEffect EffectPerFish;

        [TitleGroup("Stim Trait")]
        public Trait TraitRestriction;

        private Dictionary<Unit, int> StimCountDictionary;

        public void Perform(Unit unit)
        {
            if (!unit.PlayerHandler.BattleSystem.IsInBattle)
            {
                return;
            }

            int matchingTraitIndex = GameManager.Instance.TraitUtility.GetIndex(TraitRestriction);

            int stimCount = unit.GetSuroundingOfType(matchingTraitIndex, (int)ModifierValue);
            if (stimCount <= 0)
            {
                return;
            }

            if (StimCountDictionary == null)
            {
                StimCountDictionary = new Dictionary<Unit, int>();
            }

            if (StimCountDictionary.ContainsKey(unit))
            {
                Revert(unit);
            }

            StimCountDictionary.Add(unit, stimCount);

            float value = EffectPerFish.ModifierValue;

            EffectPerFish.ModifierValue *= StimCountDictionary[unit];
            EffectPerFish.Perform(unit);

            EffectPerFish.ModifierValue = value;
        }

        public void Revert(Unit unit)
        {
            if (StimCountDictionary == null || !StimCountDictionary.ContainsKey(unit))
            {
                return;
            }

            float value = EffectPerFish.ModifierValue;

            EffectPerFish.ModifierValue = StimCountDictionary[unit];
            EffectPerFish.Revert(unit);

            EffectPerFish.ModifierValue = value;

            StimCountDictionary.Remove(unit);
        }
    }

    #endregion

    #region Random Bonus

    public class RandomBonusEffect : IEffect
    {
        [TitleGroup("Modifier")]
        [OdinSerialize]
        public float ModifierValue { get; set; } = 2f;

        [TitleGroup("Modifier")]
        public Modifier.ModifierType ModifierType;

        [TitleGroup("Modifier")]
        public int StatAmount;

        [TitleGroup("Options")]
        public bool CanIncrease = true;

        private Dictionary<Unit, List<(StatType, Modifier)>> ModifierDictionary;

        public void Perform(Unit unit)
        {
            if (ModifierDictionary == null)
            {
                ModifierDictionary = new Dictionary<Unit, List<(StatType, Modifier)>>();
            }

            if (!ModifierDictionary.ContainsKey(unit))
            {
                ModifierDictionary.Add(unit, new List<(StatType, Modifier)>());

                for (int i = 0; i < StatAmount; i++)
                {
                    int statTypeIndex = UnityEngine.Random.Range(0, Enum.GetValues(typeof(StatType)).Length);
                    Modifier modifier = new Modifier
                    {
                        Type = ModifierType,
                        Value = ModifierValue
                    };

                    unit.UnitStats.ModifyStat((StatType)statTypeIndex, modifier);

                    ModifierDictionary[unit].Add(((StatType)statTypeIndex, modifier));
                }

            }
            else if (CanIncrease)
            {
                //ModifierDictionary[unit].Value += ModifierValue;
            }
        }

        public void Revert(Unit unit)
        {
            if (ModifierDictionary == null || !ModifierDictionary.ContainsKey(unit))
            {
                return;
            }

            for (int i = 0; i < ModifierDictionary[unit].Count; i++)
            {
                unit.UnitStats.RevertModifiedStat(ModifierDictionary[unit][i].Item1, ModifierDictionary[unit][i].Item2);
            }

            ModifierDictionary.Remove(unit);
        }
    }

    #endregion

    #region Team Bonus

    public class TeamBonusEffect : IEffect
    {
        [TitleGroup("Modifier")]
        [OdinSerialize]
        public float ModifierValue { get; set; } = 20f;

        [TitleGroup("Modifier")]
        public Modifier.ModifierType ModifierType;

        [TitleGroup("Modifier")]
        public StatType StatType;

        [TitleGroup("Options")]
        [ReadOnly]
        public bool CanIncrease = false;

        private Dictionary<Unit, (List<Unit>, Modifier)> ModifierDictionary;

        public void Perform(Unit unit)
        {
            if (ModifierDictionary == null)
            {
                ModifierDictionary = new Dictionary<Unit, (List<Unit>, Modifier)>();
            }

            if (!ModifierDictionary.ContainsKey(unit))
            {
                List<Unit> unitsOnBoard = unit.PlayerHandler.BoardSystem.UnitsOnBoard;

                ModifierDictionary.Add(unit, (unitsOnBoard, new Modifier
                {
                    Type = ModifierType,
                    Value = ModifierValue
                }));

                for (int i = 0; i < unitsOnBoard.Count; i++)
                {
                    unitsOnBoard[i].UnitStats.ModifyStat(StatType, ModifierDictionary[unit].Item2);
                }
            }
            else if (CanIncrease)
            {
                //ModifierDictionary[unit].Value += ModifierValue;
            }
        }

        public void Revert(Unit unit)
        {
            if (ModifierDictionary == null || !ModifierDictionary.ContainsKey(unit))
            {
                return;
            }

            List<Unit> unitsOnBoard = ModifierDictionary[unit].Item1;
            for (int i = 0; i < unitsOnBoard.Count; i++)
            {
                if (unitsOnBoard[i] == null)
                {
                    continue;
                }

                unitsOnBoard[i].UnitStats.RevertModifiedStat(StatType, ModifierDictionary[unit].Item2);
            }

            ModifierDictionary.Remove(unit);
        }
    }

    #endregion

    #region Gold

    public class GoldEffect : IEffect
    {
        [TitleGroup("Gold Amount")]
        [OdinSerialize]
        public float ModifierValue { get; set; } = 20f;

        public void Perform(Unit unit)
        {
            if (unit.IsEnemyUnit)
            {
                return;
            }

            unit.PlayerHandler.MoneySystem.SpawnMoney(Mathf.FloorToInt(ModifierValue), unit.transform.position);
        }

        public void Revert(Unit unit)
        {

        }
    }

    #endregion

    #region Health Condition

    public class HealthConditionalEffect : IEffect
    {
        [TitleGroup("Health Percent")]
        [OdinSerialize]
        public float ModifierValue { get; set; } = 0.5f;

        [TitleGroup("Health Percent")]
        [OdinSerialize]
        private IEffect EffectToTrigger;

        [TitleGroup("Health Percent")]
        public bool TriggerOnce = true;

        private HashSet<Unit> TriggeredUnits;

        public void Perform(Unit unit)
        {
            if (TriggeredUnits == null)
            {
                TriggeredUnits = new HashSet<Unit>();
            }

            if (TriggerOnce && TriggeredUnits.Contains(unit))
            {
                return;
            }

            if (unit.UnitHealth.HealthPercentage <= ModifierValue)
            {
                EffectToTrigger.Perform(unit);

                if (TriggerOnce)
                {
                    TriggeredUnits.Add(unit);
                }
            }
        }

        public void Revert(Unit unit)
        {
            if (TriggeredUnits == null)
            {
                return;
            }

            if (TriggerOnce)
            {
                if (!TriggeredUnits.Contains(unit))
                {
                    return;
                }

                TriggeredUnits.Remove(unit);
            }

            EffectToTrigger.Revert(unit);
        }
    }

    #endregion

    #region Add Trait

    public class AddTraitEffect : IEffect
    {
        public float ModifierValue { get; set; }

        [TitleGroup("Trait")]
        public Trait traitToAdd;

        private Dictionary<Unit, Modifier> ModifierDictionary;

        public void Perform(Unit unit)
        {
            if (ModifierDictionary == null)
            {
                ModifierDictionary = new Dictionary<Unit, Modifier>();
            }

            if (!ModifierDictionary.ContainsKey(unit))
            {
                int index = GameManager.Instance.TraitUtility.GetIndex(traitToAdd);
                unit.UnitStats.AddTrait(index);
            }
        }

        public void Revert(Unit unit)
        {
            if (ModifierDictionary == null || !ModifierDictionary.ContainsKey(unit))
            {
                return;
            }

            int index = GameManager.Instance.TraitUtility.GetIndex(traitToAdd);
            unit.UnitStats.RemoveTrait(index);

            ModifierDictionary.Remove(unit);
        }
    }

    #endregion
}

