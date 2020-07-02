using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public abstract class RoleBase : MonoBehaviour
{
    [SerializeField] protected float speed;
    [SerializeField] protected RoleAnim _anim;
    [SerializeField, Range(0.01f, 0.1f)]
    protected float stopDistance = 0.01f;
    protected Rigidbody _rigidbody;

    protected RoleData _data;

    protected RoleState _state;
    protected RoleBase firstEnemy;
    protected RoleBase enemy;

    public RoleBase Enemy => firstEnemy ? firstEnemy : enemy;
    
    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<RoleAnim>();
    }

    protected virtual void Start()
    {
    }

    protected virtual void OnEnable()
    {
    }

    protected virtual void OnDisable()
    {
    }

    public virtual void InitData(RoleData data)
    {
        _data = data;
    }

    public void Move(Vector3 dir)
    {
        _rigidbody.velocity = dir * (Time.deltaTime * speed);
    }

    private void Update()
    {
        if (firstEnemy == null)
        {
            FindEnemy();
        }
        switch (_state)
        {
            case RoleState.Idle:
            case RoleState.Attack:
                if (_rigidbody.velocity.sqrMagnitude >= stopDistance*stopDistance)
                {
                    _state = RoleState.Moving;
                    _anim.Moving();
                }
                break;
            case RoleState.Moving:
                if (_rigidbody.velocity.sqrMagnitude <= stopDistance*stopDistance)
                {
                    _state = RoleState.Idle;
                    _anim.Idle();
                }
                break;
            case RoleState.Die:
                break;
        }

        RotationHandle();
        
        switch (_state)
        {
            case RoleState.Idle:
                OnIdleUpdate();
                break;
            case RoleState.Attack:
                OnAttackUpdate();
                break;
            case RoleState.Moving:
                OnMovingUpdate();
                break;
            case RoleState.Die:
                OnDieUpdate();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected virtual void OnIdleUpdate()
    {
        if (Enemy != null)
        {
            _state = RoleState.Attack;
            _anim.Attack();
        }
    }

    protected virtual void OnAttackUpdate()
    {
        
    }

    protected virtual void OnMovingUpdate()
    {
        
    }

    protected virtual void OnDieUpdate()
    {
        
    }

    public virtual void Attack()
    {
        
    }

    public virtual void GetHit(BulletData data)
    {
        
    }

    protected virtual void FindEnemy()
    {
        var ms = RoomMagr.Current.monsters;
        RoleBase e = null;
        float d = float.MaxValue;
        foreach (var monsterBase in ms)
        {
            if (e == null) e = monsterBase;

            var temp = (monsterBase.transform.position - transform.position).sqrMagnitude;
            if (d > temp)
            {
                e = monsterBase;
                d = temp;
            }
        }

        enemy = e;
    }

    protected virtual void RotationHandle()
    {
        if (_state == RoleState.Moving)
        {
            var dir = _rigidbody.velocity;
            dir.y = 0;
            transform.DOLookAt(transform.position+dir, 0.2f).onComplete = () =>
            {
                dir.y = transform.position.y;
                transform.forward = dir;
            };
        }
        else if (Enemy != null)
        {
            var dir = Enemy.transform.position - transform.position;
            dir.y = 0;
            transform.DOLookAt(transform.position+dir, 0.2f).onComplete = () =>
            {
                dir.y = transform.position.y;
                transform.forward = dir;
            };
        }
    }

    public virtual void Die()
    {
        _state = RoleState.Die;
        _anim.Die();
    }
}