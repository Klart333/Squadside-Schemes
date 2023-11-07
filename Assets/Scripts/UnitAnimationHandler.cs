using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

public class UnitAnimationHandler : MonoBehaviour
{
    [SerializeField]
    private Unit unit;

    private Animator animator;

    private Vector3 startScale;

    private void Awake()
    {
        startScale = transform.localScale;
        animator = GetComponentInChildren<Animator>();
    }

    public void PlayMove()
    {
        animator.SetTrigger("Move");
    }

    public void PlayAttack()
    {
        animator.SetTrigger("Attack");
    }

    public void PlayUlt()
    {
        animator.SetTrigger("Ult");
    }

    public async void DoBounce(float delay = 0)
    {
        if (delay > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
        }

        transform.DOPunchScale(transform.localScale * 0.8f, 0.5f, 1, 0.1f);
    }

    public void Grow()
    {
        transform.DOScale(startScale.x * 1.2f, 1).SetEase(Ease.OutCirc);
    }

    public void ShrinkToNormal()
    {
        transform.DOKill();
        transform.DOScale(startScale.x, 0.75f).SetEase(Ease.OutCirc);
    }

    public void IncreaseStartScale(float increase)
    {
        startScale *= increase;
    }
}
