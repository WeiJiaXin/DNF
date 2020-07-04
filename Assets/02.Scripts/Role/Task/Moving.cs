using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace _02.Scripts.Role.Task
{
    [TaskCategory("Lowy")]
    public class Moving : Action
    {
        private RoleBase _role;

        public override void OnAwake()
        {
            _role = Owner.GetComponent<RoleBase>();
        }

        public override TaskStatus OnUpdate()
        {
            var axis = Joystick.Joystick.Axis;
            if (axis.sqrMagnitude > 0)
            {
                _role.Move(new Vector3(axis.x, 0, axis.y));
                _role.RotationHandle();
                return TaskStatus.Running;
            }

            return TaskStatus.Success;
        }
    }
}