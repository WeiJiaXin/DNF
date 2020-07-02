using UnityEngine;

public class ShooterRole : RoleBase
{
    [Header("Shooter")] [SerializeField] private Bullet bullet;
    [SerializeField] private Transform bulletPort;
    
    private Pool<Bullet> _bulletPool;
    protected override void Awake()
    {
        base.Awake();
        _bulletPool = PoolManager.GetPool<Bullet>(bullet.Name);
    }

    public override void Attack()
    {
        base.Attack();
        var b = _bulletPool.Pop(bulletPort);
        b.transform.localPosition=Vector3.zero;
        b.transform.localEulerAngles=Vector3.zero;
        b.transform.localScale=Vector3.one;
        b.transform.SetParent(null,true);
        var v = Enemy.transform.position - bulletPort.position;
        v.Normalize();
        b.Launch(new BulletData {atk = _data.atk, useGravity = false, velocity = v});
    }
}