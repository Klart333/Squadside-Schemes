using UnityEngine;
using UnityEngine.UI;

public class UnitManaBar : MonoBehaviour
{
    [SerializeField]
    private Image manaFill;

    public void SetMana(float percent)
    {
        manaFill.fillAmount = percent;
    }
}
