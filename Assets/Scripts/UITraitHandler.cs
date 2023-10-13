using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UITraitHandler : MonoBehaviour
{
    [SerializeField]
    private PlayerUI playerUI;

    [SerializeField]
    private UITraitDescription traitDescription;

    [SerializeField]
    private TraitHexagonUI[] hexagons;

    public UITraitDescription TraitDescriptionPanel => traitDescription;

    private async void Start()
    {
        await UniTask.WaitUntil(() => playerUI.PlayerHandler != null && playerUI.PlayerHandler.BoardSystem != null);

        playerUI.PlayerHandler.BoardSystem.OnBoardedUnitsChanged += BoardSystem_OnBoardedUnitsChanged;
    }

    private void BoardSystem_OnBoardedUnitsChanged(List<Unit> units)
    {
        Dictionary<Trait, int> traits = GameManager.Instance.TraitUtility.GetTraits(units);
        List<Trait> traitOrder = traits.Keys.ToList();
        traitOrder.Sort((x, y) => y.GetColorIndex(traits[y]).CompareTo(x.GetColorIndex(traits[x])));

        for (int i = 0; i < hexagons.Length; i++)
        {
            if (i < traitOrder.Count)
            {
                hexagons[i].Setup(traitOrder[i], traits[traitOrder[i]]);
            }
            else
            {
                hexagons[i].Empty();
            }
        }
    }
}
