using System;
using UnityEngine;

namespace Lowy.Scene
{
    public class FirstScene : MonoBehaviour
    {
        public static string scene;
        private void Awake()
        {
            SceneMag.Instance.onLoaded += OnLoadScene;
            SceneMag.Instance.LoadScene(scene);
        }

        private void OnLoadScene(SceneInfo info, bool isadditive)
        {
            if(info.name != scene)
                return;
            Destroy(this);
        }
    }
}