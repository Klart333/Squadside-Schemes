using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [Title("Setup")]
    [SerializeField]
    private GameObject healthSegmentPrefab;

    [SerializeField]
    private Transform segmentParent;

    [SerializeField]
    private Image fill;

    private int healthPerSegment = 333;

    private int maxHealth = 0;

    public void SetMaxHealth(int maxHealth)
    {
        for (int i = 0; i < segmentParent.childCount; i++)
        {
            segmentParent.GetChild(i).localScale = Vector3.one;
        }

        this.maxHealth = maxHealth;

        int segments = Mathf.FloorToInt(maxHealth / (float)healthPerSegment);
        float percentOnLast = (maxHealth - segments * healthPerSegment) / (float)healthPerSegment;

        int childCount = segmentParent.childCount;
        if (segments + 1 < childCount)
        {
            for (int i = 0; i < childCount - (segments + 1); i++)
            {
                Destroy(segmentParent.GetChild(i).gameObject);
            }
        }
        else
        {
            for (int i = 0; i < (segments + 1) - childCount; i++)
            {
                Instantiate(healthSegmentPrefab, segmentParent);
            }
        }

        segmentParent.GetChild(segmentParent.childCount - 1).localScale = new Vector3(percentOnLast, 1, 1);
    }

    public void UpdateHealthBar(float health)
    {
        float percent = health / (float)maxHealth;

        fill.fillAmount = 1.0f - percent;
    }

    public void ResetHealthBar()
    {
        fill.fillAmount = 0;
    }
}
