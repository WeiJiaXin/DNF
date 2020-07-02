using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoForDebug),true)]
public class ButtonInspector : Editor
{
//    public static void GUIDebugButton
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var methods = target.GetType().GetMethods();
        foreach (var method in methods)
        {
            if (method.GetParameters().Length > 0||
                !method.Name.EndsWith("_EB"))
                continue;
            if (GUILayout.Button(method.Name.Replace("_EB", "")))
            {
                method.Invoke(target, null);
            }
        }
    }
}