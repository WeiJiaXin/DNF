using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _02.Scripts.Role.Task
{
    [TaskCategory("Lowy")]
    public class FindEnemy : Conditional
    {
        private RoleBase _role;

        public override void OnAwake()
        {
            _role = Owner.GetComponent<RoleBase>();
        }

        public override TaskStatus OnUpdate()
        {
            _role.FindEnemy();
            return _role.Enemy == null ? TaskStatus.Failure : TaskStatus.Success;
        }
    }
}