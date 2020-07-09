using System.Collections.Generic;
using UnityEngine;

public class BloodBarMagr : MonoSingleton<BloodBarMagr>
{
    [SerializeField] private MiniBloodBar bar;

    private Dictionary<RoleBase, MiniBloodBar> bars = new Dictionary<RoleBase, MiniBloodBar>();

    public static MiniBloodBar GetBar(RoleBase role)
    {
        if (Instance == null)
            return null;
        if (!Instance.bars.ContainsKey(role))
            Instance.bars.Add(role, Instance.GenerateBar(role));
        return Instance.bars[role];
    }

    private MiniBloodBar GenerateBar(RoleBase role)
    {
        var bar = Instantiate(this.bar, transform);
        bar.SetRoleBase(role);
        return bar;
    }
}