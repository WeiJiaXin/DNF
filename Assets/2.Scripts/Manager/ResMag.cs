using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResMag : MonoSingle<ResMag>
{
    public void LoadAsync<T>(string name,Action<T> complement)where T:UnityEngine.Object
    {
        var operation = Resources.LoadAsync<T>(name);
        operation.completed += a => complement?.Invoke(operation.asset as T);
    }
}
