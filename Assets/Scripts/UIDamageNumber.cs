using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using TMPro;

public class UIDamageNumber : PooledMonoBehaviour
{
    [SerializeField]
    private TextMeshPro text;

    [SerializeField]
    private float lifetime = 1;

    private const float maxDamage = 1000;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public async void Setup(DamageInstance damageInstance, bool isCrit)
    {
        gameObject.SetActive(true);

        int damage = Mathf.RoundToInt(damageInstance.GetTotal());
        string color = damageInstance.AbilityDamage > damageInstance.AttackDamage ? "blue" : "white";
        string bold = isCrit ? "<b>" : "";

        text.text = string.Format("{0}<color={1}>{2}{3}", bold, color, damage, isCrit ? "!!" : "");

        float value = Mathf.Lerp(1, 2, damage / maxDamage);
        transform.localScale = Vector3.one * value;
        animator.speed = 1.0f / value;

        await UniTask.Delay(TimeSpan.FromSeconds(lifetime * value));

        gameObject.SetActive(false);
    }
}
