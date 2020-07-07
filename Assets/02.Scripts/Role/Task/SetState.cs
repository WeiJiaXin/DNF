using BehaviorDesigner.Runtime.Tasks;

namespace _02.Scripts.Role.Task
{
    [TaskCategory("Lowy")]
    public class SetState : Action
    {
        private RoleBase _role;
        public RoleState toState;

        public override void OnAwake()
        {
            _role = Owner.GetComponent<RoleBase>();
        }

        public override TaskStatus OnUpdate()
        {
            _role.State = toState;
            return TaskStatus.Success;
        }
    }
}