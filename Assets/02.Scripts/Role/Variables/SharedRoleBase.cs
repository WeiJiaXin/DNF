using BehaviorDesigner.Runtime;

namespace _02.Scripts.Role.Variables
{
    [System.Serializable]
    public class SharedRoleBase : SharedVariable<RoleBase>
    {
        public static implicit operator SharedRoleBase(RoleBase value) { return new SharedRoleBase { Value = value }; }
    }
}
