using System;
using System.Collections;
using System.Collections.Generic;
using Lowy.Scene;
using Lowy.UIFramework;
using UnityEngine;

public class AppMag : MonoSingle<AppMag>
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        UIManager.Push(new SelectRoleUIContent());
        SceneMag.Ins.LoadUnityMainScene();
        SceneMag.Ins.LoadScene(Scenes.City_Home);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
