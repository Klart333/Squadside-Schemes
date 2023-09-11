using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Unit : NetworkBehaviour, IInteractable
{
    public event Action<Unit> OnDeath;
    public event Action OnCombatStart;

    [TitleGroup("Data")]
    [SerializeField]
    private UnitData unitData;

    private List<(Trait, int)> cachedTraits;

    private UnitBattleController battleController;
    private MeshRenderer meshRenderer;
    private UnitHealth unitHealth;

    public UnitHealth UnitHealth => unitHealth;
    public UnitData UnitData => unitData;
    public int StarLevel { get; set; } = 0;
    public bool IsOnBoard { get; set; } = false;
    public Tile CurrentTile { get; set; }
    public PlayerHandler PlayerHandler { get; set; }

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
    public List<(Trait, int)> AppliedBoardTraits = new List<(Trait, int)>();

    private void Start()
    {
        battleController = new UnitBattleController()
        {
            Unit = this,
            UnitMoveState = new UnitMoveState(this),
            UnitAttackState = new UnitAttackState(this),
        };

        HashSet<int> indexHashSet = GameManager.Instance.TraitUtility.GetIndices(unitData.Traits).ToHashSet();

        UnitStats = new UnitStats
        {
            AttackDamage = new Stat(unitData.AttackDamage),
            AttackSpeed = new Stat(unitData.AttackSpeed),
            AbilityPower = new Stat(unitData.AbilityPower),
            AttackRange = new Stat(unitData.AttackRange),
            MaxHealth = new Stat(unitData.BaseHealth),
            MovementSpeed = new Stat(unitData.MovementSpeed),

            Traits = indexHashSet
        };

        if (cachedTraits == null)
        {
            UpdateCachedTraits(new List<Unit> { this });
        }

        unitHealth = GetComponent<UnitHealth>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        PlayerHandler.BoardSystem.OnBoardedUnitsChanged += UpdateCachedTraits;
    }

    private void UpdateCachedTraits(List<Unit> units)
    {
        if (!units.Contains(this))
        {
            return;
        }

        cachedTraits = new List<(Trait, int)>();

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
            cachedTraits.Add((trait, count));
        }

        // Check if we need to update one of the applied traits
        for (int i = 0; i < AppliedBoardTraits.Count; i++)
        {
            for (int g = 0; g < cachedTraits.Count; g++)
            {
                if (AppliedBoardTraits[i].Item1 == cachedTraits[g].Item1 && AppliedBoardTraits[i].Item2 != cachedTraits[g].Item2) // If its the same and the count is different
                {
                    cachedTraits[g].Item1.RevertAll(this);
                    cachedTraits[g].Item1.OnBoard(this, cachedTraits[g].Item2);

                    AppliedBoardTraits[i] = cachedTraits[g];
                }
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

    public void Pickup()
    {
        PlayerHandler.BoardSystem.PlacingUnit(this);
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
        battleController.Update(battle);
    }

    public bool TakeDamage(float damage)
    {
        OnTakeDamage();
        return unitHealth.TakeDamage(damage);
    }

    #endregion

    #region Death

    public void GlobalDeath()
    {
        OnDeath?.Invoke(this);

        RevertTrait();
        GameManager.Instance.DestroyServerRPC(NetworkObjectId);
    }


    public void LocalDeath()
    {
        OnDeath?.Invoke(this);

        RevertTrait();
        Destroy(gameObject);
    }
    #endregion

    #region Trait Callbacks

    public void OnPlacedOnBoard()
    {
        if (IsOnBoard) return;

        IsOnBoard = true;

        for (int i = 0; i < cachedTraits.Count; i++)
        {
            cachedTraits[i].Item1.OnBoard(this, cachedTraits[i].Item2);
            AppliedBoardTraits.Add(cachedTraits[i]);
        }
    }

    public void OnPlacedOnBench()
    {
        if (!IsOnBoard) return;

        IsOnBoard = false;

        if (cachedTraits == null)
        {
            Debug.LogError("Uh wtf man");
            UpdateCachedTraits(new List<Unit> { this });
        }

        for (int i = 0; i < cachedTraits.Count; i++)
        {
            cachedTraits[i].Item1.OnBench(this, cachedTraits[i].Item2);

            if (AppliedBoardTraits.Contains(cachedTraits[i]))
            {
                AppliedBoardTraits.Remove(cachedTraits[i]);
            }
        }
    }

    public void OnTakeDamage()
    {
        for (int i = 0; i < cachedTraits.Count; i++)
        {
            cachedTraits[i].Item1.OnTakeDamage(this, cachedTraits[i].Item2);
        }
    }

    public void OnAttack()
    {
        for (int i = 0; i < cachedTraits.Count; i++)
        {
            cachedTraits[i].Item1.OnAttack(this, cachedTraits[i].Item2);
        }
    }

    public void RevertTrait()
    {
        for (int i = 0; i < cachedTraits.Count; i++)
        {
            cachedTraits[i].Item1.RevertAll(this);
        }
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
}
