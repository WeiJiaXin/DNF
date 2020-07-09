using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniBloodBar : MonoBehaviour
{
    [SerializeField] private SliderEasing hp;

    private RectTransform rectTran;

    private RoleBase _roleBase;

    private void Awake()
    {
        rectTran = transform as RectTransform;
    }

    public void SetRoleBase(RoleBase role)
    {
        _roleBase = role;
        hp.value = 1;
    }

    private void Update()
    {
        var pos = GameCamera.current.WorldToUI(_roleBase.Head.position);
        rectTran.anchoredPosition = pos;
    }

    public void DeductHp()
    {
        hp.value = _roleBase.Data.Hp / (float) _roleBase.Data.MaxHp;
    }
}