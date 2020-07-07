using BehaviorDesigner.Runtime.Tasks;

namespace _02.Scripts.Role.Task
{
    [TaskCategory("Lowy")]
    public class Attack : Action
    {
        private RoleBase _role;

        public override void OnAwake()
        {
            _role = Owner.GetComponent<RoleBase>();
        }

        public override TaskStatus OnUpdate()
        {
            _role.RotationHandle();
            return TaskStatus.Running;
        }
    }
}