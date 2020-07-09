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
        b.Init(true);
        var v = (Enemy.transform.position - bulletPort.position).normalized;
        b.Launch(this,new BulletData {atk = _data.Atk, useGravity = false, velocity = v});
    }
}