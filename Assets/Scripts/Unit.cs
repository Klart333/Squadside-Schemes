using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
using System;

public class Unit : NetworkBehaviour, IInteractable
{
    public event Action<Unit> OnDeath;
    public event Action OnCombatStart;
    public event Action OnAttack;
    public event Action OnTakeDamage;
    public event Action OnDamageDone;
    public event Action OnKill;

    [TitleGroup("Data")]
    [SerializeField]
    private UnitData unitData;

    public Dictionary<Trait, int> AppliedBoardTraits = new Dictionary<Trait, int>();

    private Dictionary<Trait, int> cachedTraits;

    private UnitAnimationHandler unitAnimator;
    private MeshRenderer meshRenderer;
    private UnitHealth unitHealth;
    private UnitManaBar unitMana;
    private UnitItems unitItems;

    public UnitAnimationHandler UnitAnimator => unitAnimator;
    public UnitHealth UnitHealth => unitHealth;
    public UnitManaBar UnitMana => unitMana;
    public UnitItems UnitItems => unitItems;
    public UnitData UnitData => unitData;
    public int StarLevel { get; set; } = 0;
    public bool IsOnBoard { get; set; } = false;
    public bool IsInitialized { get; set; } = false;
    public bool IsEnemyUnit { get; set; } = false;
    public Tile CurrentTile { get; set; }
    public ItemData[] ItemSlots { get; set; }
    public PlayerHandler PlayerHandler { get; set; }
    public DamageInstance LastDamageDone { get; set; }
    public UnitBattleController BattleController { get; private set; }

    [Title("Stats"), ShowInInspector, ReadOnly]
    public UnitStats UnitStats { get; set; }

    private bool interactable = true;
    public bool IsInteractable
    {
        get
        {
            return !PlayerHandler.BattleSystem.IsInBattle && interactable;
        }

        set
        {
            interactable = value;
        }
    }
    public int ItemCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < ItemSlots.Length; i++)
            {
                if (ItemSlots[i] == null)
                {
                    break;
                }
                count++;
            }

            return count;
        }
    }

    private void Start()
    {
        ItemSlots = new ItemData[3];

        BattleController = new UnitBattleController()
        {
            Unit = this,
            UnitMoveState = new UnitMoveState(this),
            UnitAttackState = new UnitAttackState(this),
            UnitUltimateState = new UnitUltimateState(this),
        };

        HashSet<int> indexHashSet = GameManager.Instance.TraitUtility.GetIndices(unitData.Traits).ToHashSet();

        UnitStats = new UnitStats
        {
            AttackDamage = new Stat(unitData.AttackDamage),
            AttackSpeed = new Stat(unitData.AttackSpeed),
            AttackRange = new Stat(unitData.AttackRange),
            AbilityPower = new Stat(unitData.AbilityPower),
            Mana = new Stat(0),
            MaxMana = new Stat(unitData.MaxMana),
            CritChance = new Stat(unitData.CritChance),
            CritMultiplier = new Stat(unitData.CritMultiplier),
            Armor = new Stat(unitData.Armor),
            MagicResist = new Stat(unitData.MagicResist),
            MaxHealth = new Stat(unitData.BaseHealth),
            MovementSpeed = new Stat(unitData.MovementSpeed),
            Omnivamp = new Stat(unitData.Omnivamp),

            Traits = indexHashSet
        };

        if (cachedTraits == null)
        {
            UpdateCachedTraits(new List<Unit> { this });
        }

        unitAnimator = GetComponentInChildren<UnitAnimationHandler>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        unitMana = GetComponentInChildren<UnitManaBar>();
        unitHealth = GetComponent<UnitHealth>();
        unitItems = GetComponent<UnitItems>();

        if (unitMana)
        {
            UnitStats.Mana.OnValueChanged += UpdateManaBar;
            UnitStats.Mana.AddModifier(new Modifier { Value = UnitData.Mana, Type = Modifier.ModifierType.Additive });
        }

        if (IsOwner)
        {
            PlayerHandler.BoardSystem.OnBoardedUnitsChanged += UpdateCachedTraits;
        }

        IsInitialized = true;
    }

    private void OnDisable()
    {
        UnitStats.Mana.OnValueChanged -= UpdateManaBar;

        if (IsOwner)
        {
            PlayerHandler.BoardSystem.OnBoardedUnitsChanged -= UpdateCachedTraits;
        }
    }

    private void UpdateManaBar()
    {
        if (unitMana)
        {
            unitMana.SetMana(UnitStats.Mana.Value / UnitStats.MaxMana.Value);
        }
    }

    public void UpdateCachedTraits(List<Unit> units)
    {
        if (!units.Contains(this))
        {
            return;
        }

        cachedTraits = new Dictionary<Trait, int>();

        foreach (int traitIndex in UnitStats.Traits)
        {
            Trait trait = GameManager.Instance.TraitUtility.Get(traitIndex);

            HashSet<string> countedUnits = new HashSet<string>();
            int count = 0;
            for (int i = 0; i < units.Count; i++)
            {
                if (countedUnits.Contains(units[i].UnitData.Name))
                {
                    continue;
                }

                countedUnits.Add(units[i].UnitData.Name);
                if (units[i].UnitStats.Traits.Contains(traitIndex))
                {
                    count++;
                }
            }

            //print("Trait: " + trait.Name + " has count: " + count);
            cachedTraits.Add(trait, count);
        }

        // Check if we need to update one of the applied traits
        foreach (Trait cachedTrait in cachedTraits.Keys)
        {
            if (AppliedBoardTraits.ContainsKey(cachedTrait) && cachedTraits[cachedTrait] != AppliedBoardTraits[cachedTrait]) // If the Count is different
            {
                cachedTrait.RevertAll(this);
                cachedTrait.OnBoard(this, cachedTraits[cachedTrait]);

                AppliedBoardTraits[cachedTrait] = cachedTraits[cachedTrait];
            }
        }
    }

    #region Interact

    public void StartInteract()
    {
        if (!meshRenderer)
        {
            return;
        }

        meshRenderer.gameObject.layer = LayerMask.NameToLayer("Highlight");
    }

    public void EndInteract()
    {
        if (!meshRenderer)
        {
            return;
        }

        meshRenderer.gameObject.layer = LayerMask.NameToLayer("Unit");
    }

    public bool Pickup()
    {
        PlayerHandler.BoardSystem.PlacingUnit(this);
        return false;
    }

    public void Place()
    {
        PlayerHandler.BoardSystem.PlaceUnit(this);
    }

    #endregion

    #region Upgrade

    [ServerRpc(RequireOwnership = false)]
    public void UpgradeStarLevelServerRPC()
    {
        UpgradeStarLevelClientRPC();
    }

    [ClientRpc]
    private void UpgradeStarLevelClientRPC()
    {
        UpgradeStarLevel();
    }

    public async void UpgradeStarLevel()
    {
        await UniTask.WaitUntil(() => UnitStats != null).TimeoutWithoutException(TimeSpan.FromSeconds(1));

        transform.localScale *= 1.2f;
        StarLevel++;

        UnitStats.AttackDamage.BaseValue *= 1.6f;
        UnitStats.MaxHealth.BaseValue *= 1.6f;
    }

    #endregion

    #region Combat

    public void StartCombat()
    {
        OnCombatStart?.Invoke();
    }

    public void UpdateBattle(Battle battle)
    {
        BattleController.Update(battle);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>If the unit died</returns>
    public bool TakeDamage(DamageInstance damage, out DamageInstance damageDone)
    {
        bool died = unitHealth.TakeDamage(damage, out damageDone);
        OnUnitTakeDamage(damageDone);

        return died;
    }

    #endregion

    #region Death

    public void GlobalDeath()
    {
        OnDeath?.Invoke(this);

        if (CurrentTile != null)
        {
            CurrentTile.CurrentUnit = null;
        }

        RevertTrait();
        GameManager.Instance.DestroyServerRPC(NetworkObjectId);
    }

    public void LocalDeath()
    {
        OnDeath?.Invoke(this);

        if (CurrentTile != null)
        {
            CurrentTile.CurrentUnit = null;
        }

        RevertTrait();
        Destroy(gameObject);
    }
    #endregion

    #region Trait Callbacks

    public void OnPlacedOnBoard()
    {
        if (IsOnBoard) return;

        IsOnBoard = true;

        foreach (Trait trait in cachedTraits.Keys)
        {
            trait.OnBoard(this, cachedTraits[trait]);

            AppliedBoardTraits.Add(trait, cachedTraits[trait]);
        }
    }

    public void OnPlacedOnBench()
    {
        if (!IsOnBoard) return;

        IsOnBoard = false;

        foreach (Trait trait in cachedTraits.Keys)
        {
            trait.OnBench(this, cachedTraits[trait]);

            if (AppliedBoardTraits.ContainsKey(trait))
            {
                AppliedBoardTraits.Remove(trait);
            }
        }
    }

    public void OnUnitTakeDamage(DamageInstance damageTaken)
    {
        OnTakeDamage?.Invoke();

        UnitStats.Mana.AddModifier((int)(damageTaken.GetTotal() / 10.0f));

        foreach (Trait trait in cachedTraits.Keys)
        {
            trait.OnTakeDamage(this, cachedTraits[trait]);
        }
    }

    public void OnUnitDoneDamage(DamageInstance damageInstance)
    {
        LastDamageDone = damageInstance;

        OnDamageDone?.Invoke();

        UnitHealth.AddHealth(UnitStats.Omnivamp.Value * damageInstance.GetTotal());

        foreach (Trait trait in cachedTraits.Keys)
        {
            trait.OnDoneDamage(this, cachedTraits[trait]);
        }
    }

    public void OnUnitAttack()
    {
        OnAttack?.Invoke();

        UnitStats.Mana.AddModifier(10);
        
        foreach (Trait trait in cachedTraits.Keys)
        {
            trait.OnAttack(this, cachedTraits[trait]);
        }
    }

    public void OnUnitKill()
    {
        OnKill?.Invoke();

        foreach (Trait trait in cachedTraits.Keys)
        {
            trait.OnKill(this, cachedTraits[trait]);
        }
    }

    public void RevertTrait()
    {
        foreach (Trait trait in cachedTraits.Keys)
        {
            trait.RevertAll(this);
        }
    }

    #endregion

    #region Items

    public bool ApplyItem(ItemData item)
    {
        if (item == null)
        {
            return false;
        }

        if (item.IsComponent)
        {
            for (int i = 0; i < ItemSlots.Length; i++)
            {
                if (ItemSlots[i] != null && ItemSlots[i].IsComponent)
                {
                    ItemData combined = GameManager.Instance.ItemDataUtility.CombineItems(item, ItemSlots[i]);
                    RemoveItem(ItemSlots[i]);

                    return ApplyItem(combined);
                }
            }
        }

        int index = -1;
        for (int i = 0; i < ItemSlots.Length; i++)
        {
            if (ItemSlots[i] == null)
            {
                ItemSlots[i] = item;
                index = i;
                break;
            }
        }

        if (index == -1)
        {
            return false;
        }

        UnitItems.ApplyItem(item, index);

        if (IsOnBoard && item.name.Contains("Emblem"))
        {
            PlayerHandler.BoardSystem.UpdateBoardUnits();
        }

        return true;
    }

    public void RemoveItem(ItemData item)
    {
        int index = -1;
        for (int i = 0; i < ItemSlots.Length; i++)
        {
            if (ItemSlots[i] == null)
            {
                break;
            }

            if (ItemSlots[i].name == item.name)
            {
                ItemSlots[i] = null;
                index = i;
                break;
            }
        }

        if (index == -1)
        {
            Debug.LogError("Trying to remove item unit does not have ???");
            return;
        }

        UnitItems.RemoveItem(item, index);
    }

    #endregion

    #region Show/Hide

    public void ToggleVisibility(bool value)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(value);
        }
    }

    #endregion

    #region Utility

    public bool IsStrongest(int matchingTraitIndex = -1)
    {
        List<Unit> units = PlayerHandler.BoardSystem.UnitsOnBoard;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] == this || this.IsEnemyUnit != units[i].IsEnemyUnit)
            {
                continue;
            }

            if (matchingTraitIndex != -1 && !units[i].UnitStats.Traits.Contains(matchingTraitIndex))
            {
                continue;
            }

            int myCount = ItemCount;
            int theirCount = units[i].ItemCount;
            if (myCount < theirCount)
            {
                return false;
            }
            else if (myCount > theirCount)
            {
                continue;
            }

            if (StarLevel < units[i].StarLevel)
            {
                return false;
            }
            else if (StarLevel > units[i].StarLevel)
            {
                continue;
            }

            if (UnitData.Cost < units[i].UnitData.Cost)
            {
                return false;
            }
            else if (UnitData.Cost > units[i].UnitData.Cost)
            {
                continue;
            }
        }

        return true;
    }

    public int GetSuroundingOfType(int traitIndex, int distance)
    {
        List<Unit> units = PlayerHandler.BoardSystem.UnitsOnBoard;

        int count = 0;
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] == this || IsEnemyUnit != units[i].IsEnemyUnit)
            {
                continue;
            }

            if (traitIndex != -1 && !units[i].UnitStats.Traits.Contains(traitIndex))
            {
                continue;
            }

            if ((units[i].CurrentTile.WorldPosition - CurrentTile.WorldPosition).sqrMagnitude < distance * distance)
            {
                count++;
            }
        }

        return count;
    }

    #endregion
}
