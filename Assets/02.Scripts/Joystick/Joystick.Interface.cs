using UnityEngine;
using UnityEngine.UI;

namespace _02.Scripts.Joystick
{
    public partial class Joystick
    {
        private static Joystick current;
        public static Joystick Current => current;

        private static bool _enable;

        public static bool Enable
        {
            get => _enable;
            set
            {
                _enable = value;
                Current.enabled = _enable;
            }
        }

        public static Vector2 Axis => Current.axis;
    }
}