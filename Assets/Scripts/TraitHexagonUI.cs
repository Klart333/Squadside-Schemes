using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TraitHexagonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Title("Handler")]
    [SerializeField]
    private UITraitHandler traitHandler;

    [Title("Components")]
    [SerializeField]
    private GameObject unitCountPanel;

    [SerializeField]
    private TextMeshProUGUI unitCountText;

    [SerializeField]
    private TextMeshProUGUI traitBreakpointsText;

    [SerializeField]
    private TextMeshProUGUI traitTitleText;

    [SerializeField]
    private Image hexagonFillBackground;

    [SerializeField]
    private Image hexagonOutline;

    [SerializeField]
    private Image traitIcon;

    [SerializeField]
    private Image highlight;

    [SerializeField]
    private Color[] breakpointColors;

    [Title("The Flexible Background")]
    [SerializeField]
    private UIFlexibleBackground flexibleBackground;

    private Trait currentTrait;

    public void Setup(Trait trait, int unitCount)
    {
        gameObject.SetActive(true);

        currentTrait = trait;

        traitTitleText.text = trait.Name;
        traitIcon.sprite = trait.Icon;

        int activatedIndex = 0;
        for (int i = trait.TraitBreakpoints.Length - 1; i >= 0; i--)
        {
            if (unitCount >= trait.TraitBreakpoints[i].UnitCount)
            {
                activatedIndex = i + 1;

                if (trait.Exclusive)
                {
                    if (unitCount > 1)
                    {
                        activatedIndex = 0;
                    }
                    else
                    {
                        activatedIndex = 3;
                    }
                }
                break;
            }
        }


        hexagonFillBackground.color = breakpointColors[Mathf.Min(breakpointColors.Length - 1, activatedIndex)];

        if (activatedIndex == 0)
        {

            ShowUnActive(trait, unitCount);
            return;
        }

        // Active
        ToggleElements(true);
        flexibleBackground.ResizeToTrait(trait, true);

        unitCountText.text = unitCount.ToString();

        traitIcon.color = Color.gray;

        string breakpoints = "";

        for (int i = 0; i < trait.TraitBreakpoints.Length; i++)
        {
            if (i == activatedIndex - 1)
            {
                if (i == 0)
                {
                    breakpoints = string.Format("<color=white>{0}</color> ", trait.TraitBreakpoints[i].UnitCount);
                    continue;
                }

                breakpoints = string.Format("{0} > <color=white>{1}</color> ", breakpoints, trait.TraitBreakpoints[i].UnitCount);
            }
            else
            {
                if (i == 0)
                {
                    breakpoints = string.Format("{0} ", trait.TraitBreakpoints[i].UnitCount);
                    continue;
                }

                breakpoints = string.Format("{0} > {1} ", breakpoints, trait.TraitBreakpoints[i].UnitCount);
            }
        }

        traitBreakpointsText.text = breakpoints;
    }

    private void ShowUnActive(Trait trait, int unitCount)
    {
        ToggleElements(false);
        flexibleBackground.ResizeToTrait(trait, false);

        traitIcon.color = Color.black;
        traitBreakpointsText.text = string.Format("{0}/{1}", unitCount, trait.TraitBreakpoints[0].UnitCount);
    }

    private void ToggleElements(bool enabled)
    {
        unitCountPanel.gameObject.SetActive(enabled);
        hexagonOutline.enabled = !enabled;
    }

    public void Empty()
    {
        gameObject.SetActive(false);
        currentTrait = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentTrait == null)
        {
            return;
        }

        traitHandler.TraitDescriptionPanel.gameObject.SetActive(true);
        highlight.gameObject.SetActive(true);

        traitHandler.TraitDescriptionPanel.Setup(currentTrait);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        traitHandler.TraitDescriptionPanel.gameObject.SetActive(false);
        highlight.gameObject.SetActive(false);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        RectTransform rectTransform = traitHandler.TraitDescriptionPanel.transform as RectTransform;
        rectTransform.position = eventData.position + new Vector2(rectTransform.rect.size.x, -rectTransform.rect.size.y) / 2.0f;
    }
}
