using _02.Scripts.Joystick;
using UnityEngine;

public class Player : MonoBehaviour
{
    private RoleBase _role;

    private void Awake()
    {
        _role = GetComponent<RoleBase>();
    }

    private void Update()
    {
        var axis = Joystick.Axis;
        _role.Move(new Vector3(axis.x, 0, axis.y));
    }
}