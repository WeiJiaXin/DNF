using UnityEngine;
using System.Collections;

namespace CompleteProject
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;            // The position that that camera will be following.
        public float smoothing = 5f;        // The speed with which the camera will be following.

        void FixedUpdate ()
        {
            if (target == null)
            {
                if (DungeonsMagr.Instance)
                    target = DungeonsMagr.Instance.player?.transform;
            }
            // Create a postion the camera is aiming for based on the offset from the target.
            Vector3 targetCamPos = target.position + transform.forward * -50;

            // Smoothly interpolate between the camera's current position and it's target position.
            transform.position = Vector3.Lerp (transform.position, targetCamPos, smoothing * Time.deltaTime);
        }
    }
}