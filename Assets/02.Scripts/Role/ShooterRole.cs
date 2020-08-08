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
        if (Enemy == null)
            return;
        base.Attack();
        var b = _bulletPool.Pop(bulletPort);
        b.Init(true);
        var enemyPos = Enemy.transform.position;
        enemyPos.y += Enemy.height * 0.6f;
        if (bulletPort.position.y > Enemy.transform.position.y &&
            bulletPort.position.y < enemyPos.y)
            enemyPos.y = bulletPort.position.y;
        var v = (enemyPos - bulletPort.position).normalized;
        b.Launch(this, new BulletData {atk = _data.Atk, useGravity = false, velocity = v});
    }
}