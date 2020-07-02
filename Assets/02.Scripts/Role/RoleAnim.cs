using System;
using UnityEngine;

public class RoleAnim : MonoBehaviour
{
    private static readonly int State = Animator.StringToHash("State");
    private static readonly int Die1 = Animator.StringToHash("Die");

    private Animator _animator;
    private RoleBase _roleBase;

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _roleBase = GetComponentInParent<RoleBase>();
    }

    public void OnAttack(AnimationEvent e)
    {
        _roleBase.Attack();
    }

    public virtual void Moving()
    {
        _animator.SetInteger(State, 1);
    }

    public virtual void Idle()
    {
        _animator.SetInteger(State, 0);
    }

    public virtual void Die()
    {
        _animator.SetBool(Die1, true);
    }

    public virtual void Attack()
    {
        _animator.SetInteger(State, 2);
    }
}