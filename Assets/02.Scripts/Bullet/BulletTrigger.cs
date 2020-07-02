using System;
using UnityEngine;

public class BulletTrigger : MonoForDebug
{
    public RoleBase role { get; private set; }

    private void Awake()
    {
        role = GetComponentInParent<RoleBase>();
    }
}