using BehaviorDesigner.Runtime.Tasks;

namespace _02.Scripts.Role.Task
{
    [TaskCategory("Lowy")]
    public class IsInState : Conditional
    {
        private RoleBase _role;
        public RoleState _state;

        public override void OnAwake()
        {
            _role = Owner.GetComponent<RoleBase>();
        }

        public override TaskStatus OnUpdate()
        {
            return _role.State == _state ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}