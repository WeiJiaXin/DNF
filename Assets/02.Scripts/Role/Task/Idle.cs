using BehaviorDesigner.Runtime.Tasks;

namespace _02.Scripts.Role.Task
{
    [TaskCategory("Lowy")]
    public class Idle : Action
    {
        private RoleBase _role;

        public override void OnAwake()
        {
            _role = Owner.GetComponent<RoleBase>();
        }

        public override TaskStatus OnUpdate()
        {
            if (_role.State != RoleState.Idle)
                _role.Anim.Idle();
            _role.State = RoleState.Idle;
            return TaskStatus.Running;
        }
    }
}