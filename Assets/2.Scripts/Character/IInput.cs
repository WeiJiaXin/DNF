using System;
using UnityEngine;

public interface IInput
{
    event Action Jump;
    Vector2 GetAxis();
}