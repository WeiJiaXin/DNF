using Lowy.Event;
using UnityEngine;

public class MonsterBase : RoleBase
{
    public override void InitData(RoleData data)
    {
        base.InitData(data);
        EventManager.Dispatch(new MonsterInitedEve {monster = this});
    }

    public override void FindEnemy()
    {
        enemy = DungeonsMagr.Instance.player;
    }

    public override void Die()
    {
        base.Die();
        EventManager.Dispatch(new MonsterDieEve {monster = this});
    }
}