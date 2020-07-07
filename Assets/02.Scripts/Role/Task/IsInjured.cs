using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _02.Scripts.Role.Task
{
    [TaskCategory("Lowy")]
    public class IsInjured : Conditional
    {
        private RoleBase _role;
        public SharedVector3 injuredSourcePos;

        public override void OnAwake()
        {
            _role = Owner.GetComponent<RoleBase>();
        }

        public override TaskStatus OnUpdate()
        {
            if (_role.InjuredSource == null)
                return TaskStatus.Failure;
            else
            {
                injuredSourcePos.Value = _role.InjuredSource.transform.position;
                return TaskStatus.Success;
            }
        }
    }
}