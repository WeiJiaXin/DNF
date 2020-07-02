using DG.Tweening;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public static GameCamera current;
    public static Camera mainCam;
    public static Camera uiCam;

    private void Awake()
    {
        current = this;
        mainCam = GetComponent<Camera>();
        var cams = FindObjectsOfType<Camera>();
        foreach (var cam in cams)
        {
            if (cam.name.ToLower().StartsWith("uicam"))
            {
                uiCam = cam;
                break;
            }
        }
    }

    public static float Width => uiCam.orthographicSize * (Screen.width / (float) Screen.height) * 2;
    public static float Height => uiCam.orthographicSize * 2;

    public static void CameraShake(float duration = 0.2f, float? strength = null, bool needVibrate = true)
    {
        var camera = Camera.main;
        camera.DOComplete();
        var originCamera = camera.transform.position;
        camera.transform.DOShakePosition(duration,
            new Vector3(Random.Range(0, 2) == 0 ? 1 : -1, Random.Range(0, 2) == 0 ? 1 : -1, 0f) *
            (strength ?? Random.Range(0.1f, 0.15f)),
            Random.Range(20, 30), 90,
            false, false);
    }

    public Vector2 WorldToUI(Transform tran)
    {
        var point = (Vector2) mainCam.WorldToScreenPoint(tran.position) - new Vector2(Screen.width, Screen.height) * 0.5f;
        point *= 1080f / Screen.width;
        return point;
    }
}