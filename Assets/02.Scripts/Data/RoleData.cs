using System.Collections.Generic;
using UnityEngine;

public enum RoleDataEnum
{
    hp=1,
    maxHp,
    mp,
    maxMp,
    atk,
    def,
}
public class RoleData
{
    private Dictionary<RoleDataEnum, float> _data;
    public Dictionary<RoleDataEnum, float> Data => _data;

    public RoleData()
    {
        _data = new Dictionary<RoleDataEnum, float>
        {
            {RoleDataEnum.hp, 100},
            {RoleDataEnum.maxHp, 100},
            {RoleDataEnum.mp, 100},
            {RoleDataEnum.maxMp, 100},
            {RoleDataEnum.atk, 10},
            {RoleDataEnum.def, 0}
        };
    }

    public int Hp
    {
        get => (int) _data[RoleDataEnum.hp];
        set => _data[RoleDataEnum.hp] = Mathf.Max(0, value);
    }

    public int MaxHp => (int) _data[RoleDataEnum.maxHp];
    public int Mp => (int) _data[RoleDataEnum.mp];
    public int MaxMp => (int) _data[RoleDataEnum.maxMp];
    public int Atk => (int) _data[RoleDataEnum.atk];
    public int Def => (int) _data[RoleDataEnum.def];
}