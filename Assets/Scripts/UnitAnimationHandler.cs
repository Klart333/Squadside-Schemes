using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimationHandler : MonoBehaviour
{
    [SerializeField]
    private Unit unit;

    private Animator animator;

    private void Awake()
    {
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
}
