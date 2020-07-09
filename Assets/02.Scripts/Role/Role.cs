using UnityEngine;

public static class Role
{
    public static RoleBase LoadPlayer(Transform parent)
    {
        var prefab = Resources.Load<RoleBase>("Role/01.Player/Shooter");
        var role = Object.Instantiate(prefab, parent);
        //role init data
        role.InitData(new RoleData());
        return role;
    }
}