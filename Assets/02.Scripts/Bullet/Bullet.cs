using System;
using UnityEngine;

public class BulletData
{
    public Vector3 velocity;
    public bool useGravity;
    public int atk;
}

public struct InjuredData
{
    public RoleBase source;
    public int atk;

    public static InjuredData GetData(RoleBase s,BulletData d)
    {
        return new InjuredData() {source = s, atk = d.atk};
    }
}

public class Bullet : MonoForDebug, IPoolObject
{
    public string Name => $"Bullet/{nameof(Bullet)}";

    //
    public float speed = 1;
    private Rigidbody _rig;

    private RoleBase _source;
    private BulletData _data;

    private void Awake()
    {
        _rig = GetComponent<Rigidbody>();
    }

    public void Launch(RoleBase source, BulletData data)
    {
        _source = source;
        _data = data;
        _rig.velocity = _data.velocity * speed;
        _rig.useGravity = _data.useGravity;
    }

    private void OnTriggerEnter(Collider other)
    {
        //同在子弹触发层,tag标识敌人还是自己(可触发或忽略)
        var bulletTrigger = other.GetComponent<BulletTrigger>();
        if (bulletTrigger == null)
            return;
        if (bulletTrigger.CompareTag(tag))
            return;
        bulletTrigger.role.Injured(InjuredData.GetData(_source, _data));
        Recycle();
    }

    public void Recycle()
    {
        _rig.velocity = Vector3.zero;
        PoolManager.GetPool<Bullet>(Name).Push(this);
    }

    public void Init(bool isPlayer)
    {
        gameObject.tag = isPlayer ? "BulletPlayer" : "BulletMonster";
        var tran = transform;
        tran.localPosition=Vector3.zero;
        tran.localEulerAngles=Vector3.zero;
        tran.localScale=Vector3.one;
        tran.SetParent(null,true);
    }
}