using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameTimer;
using UnityEngine;
using UnityEngine.AI;

public abstract class RoleBase : MonoBehaviour
{
    [SerializeField] protected float speed;
    [SerializeField] protected RoleAnim _anim;
    [SerializeField, Range(0.01f, 0.1f)] protected float stopDistance = 0.01f;
    protected CharacterController cc;

    protected RoleData _data;

    protected RoleBase firstEnemy;
    protected RoleBase enemy;

    private TimeHandle injuredSourceTimer;//伤害源计时器
    public RoleBase InjuredSource { get; set; }//如果受伤了,伤害源显示两秒

    public RoleAnim Anim => _anim;
    public RoleState State { get; set; }

    public RoleBase Enemy => firstEnemy ? firstEnemy : enemy;

    protected virtual void Awake()
    {
        cc = GetComponent<CharacterController>();
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
        cc.Move(dir * (Time.deltaTime * speed));
    }

    public virtual void Attack()
    {
    }

    public virtual void Injured(InjuredData data)
    {
        if (injuredSourceTimer == null)
        {
            injuredSourceTimer = Timer.Start(2);
            injuredSourceTimer.onEnd += h =>
            {
                InjuredSource = null;
                injuredSourceTimer = null;
            };
            InjuredSource = data.source;
        }
    }

    public virtual void FindEnemy()
    {
        if (firstEnemy != null)
            return;
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

    public virtual void RotationHandle()
    {
        if (State == RoleState.Moving)
        {
            var dir = cc.velocity;
            dir.y = 0;
            transform.DOLookAt(transform.position + dir, 0.2f).onComplete = () =>
            {
                dir.y = transform.position.y;
                transform.forward = dir;
            };
        }
        else if (Enemy != null)
        {
            var dir = Enemy.transform.position - transform.position;
            dir.y = 0;
            transform.DOLookAt(transform.position + dir, 0.2f).onComplete = () =>
            {
                dir.y = transform.position.y;
                transform.forward = dir;
            };
        }
    }

    public virtual void Die()
    {
        State = RoleState.Die;
    }
}