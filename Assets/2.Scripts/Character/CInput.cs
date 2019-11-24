using System;
using UnityEngine;

public class MacInput:MonoBehaviour,IInput
{
    [SerializeField] private float doubleTime = 0.1f;
    private KeyCode lastKey;
    private float clickTime;
    private Vector2 axis;
    //
    private RoleBase role;
    //
    public event Action Jump;
    //
    private void Awake()
    {
        role = GetComponent<RoleBase>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            axis.y += 1;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            axis.y -= 1;
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            axis.y -= 1;
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            axis.y += 1;
        }

        axis.y = Mathf.Clamp(axis.y, -1f, 1f);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            
        }
    }

    public Vector2 GetAxis()
    {
        return axis;
    }
}