using DG.Tweening;
using GameTimer;
using Lowy.Event;
using UnityEngine;
using UnityEngine.AI;

public class MonsterBase : RoleBase
{
    protected MiniBloodBar bloodBar;

    protected override void Awake()
    {
        base.Awake();
        bloodBar = BloodBarMagr.GetBar(this);
    }

    public override void InitData(RoleData data)
    {
        base.InitData(data);
        EventManager.Dispatch(new MonsterInitedEve {monster = this});
    }

    public override void FindEnemy()
    {
        enemy = DungeonsMagr.Instance.player;
    }

    public override void Injured(InjuredData data)
    {
        base.Injured(data);
        bloodBar.DeductHp();
    }

    public override void Die()
    {
        base.Die();
        EventManager.Dispatch(new MonsterDieEve {monster = this});

        TimerStatic.Schedule(1, ClearBody);
    }

    protected virtual void ClearBody(TimeHandle obj)
    {
        BloodBarMagr.DestroyBar(this);
        var navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent)
            navMeshAgent.enabled = false;
        transform.DOMoveY(transform.position.y - head.localPosition.y, 0.5f).onComplete =
            () => { Destroy(gameObject); };
    }
}