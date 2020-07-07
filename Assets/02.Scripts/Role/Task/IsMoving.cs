﻿using BehaviorDesigner.Runtime.Tasks;

namespace _02.Scripts.Role.Task
{
    [TaskCategory("Lowy")]
    public class IsMoving : Conditional
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
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}