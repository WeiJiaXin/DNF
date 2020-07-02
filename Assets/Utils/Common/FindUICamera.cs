using System;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class FindUICamera : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Canvas>().worldCamera = GameCamera.uiCam;
    }
}