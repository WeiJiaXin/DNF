using UnityEngine;
using System.Collections;

namespace CompleteProject
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;            // The position that that camera will be following.
        public float smoothing = 5f;        // The speed with which the camera will be following.

        private Rect bord;

        void FixedUpdate ()
        {
            if (target == null)
            {
                if (DungeonsMagr.Instance)
                    target = DungeonsMagr.Instance.player?.transform;
            }
            if(target == null)
                return;
            // Create a postion the camera is aiming for based on the offset from the target.
            Vector3 targetCamPos = target.position;
            if (RoomMagr.Current == null)
                bord = Rect.zero;
            else
                bord = RoomMagr.Current.Bord;
            targetCamPos.x = Mathf.Clamp(targetCamPos.x, bord.xMin, bord.xMax);
            targetCamPos.z = Mathf.Clamp(targetCamPos.z, bord.yMin, bord.yMax);
            targetCamPos += transform.forward * -50;

            // Smoothly interpolate between the camera's current position and it's target position.
            transform.position = Vector3.Lerp (transform.position, targetCamPos, smoothing * Time.deltaTime);
        }
    }
}