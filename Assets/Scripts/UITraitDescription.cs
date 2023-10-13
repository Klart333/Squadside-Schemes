using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITraitDescription : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI traitDescriptionText;

    [SerializeField]
    private TextMeshProUGUI traitTitle;

    [SerializeField]
    private Image traitIcon;

    public void Setup(Trait trait)
    {
        traitTitle.text = trait.Name;
        traitDescriptionText.text = trait.Description;
        traitIcon.sprite = trait.Icon;
    }
}
