using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lowy.Event;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class EventManagerWindow
{
    private ReorderableList eventList;
    private List<string> names;
    private Dictionary<string, bool> names2Valid;

    private void DrawEventList()
    {
        if (eventList == null)
            InitData();
        eventList.drawElementCallback = DrawEventsItem;
        eventList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Events");
        eventList.displayAdd = false;
        eventList.displayRemove = false;
        eventList.onSelectCallback=SelectEventItem;
        eventList.DoLayoutList();
    }

    private void InitData()
    {
        names = FindEvent();
        names2Valid = CheckValid(names);
        eventList = new ReorderableList(names, typeof(string));
    }

    private List<string> FindEvent()
    {
        Type[] ts = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IEvent))))
            .ToArray();
        List<string> names=new List<string>(ts.Length);
        foreach (var type in ts)
        {
            names.Add(type.Name);
        }

        return names;
    }

    private Dictionary<string,bool> CheckValid(List<string> names)
    {
        Dictionary<string,bool> valid=new Dictionary<string, bool>();
        string path = $"{Application.dataPath}/{_content.cs_path}/{{0}}.cs";
        foreach (var n in names)
        {
            valid.Add(n, File.Exists(String.Format(path, n)));
        }

        return valid;
    }

    private void DrawEventsItem(Rect rect,int index,bool isActive,bool isFocused)
    {
        var element = names[index];
        rect.height -= 4;
        rect.width /= 5f;
        rect.width *= 3;
        rect.y += 2;
        EditorGUI.TextField(rect, element);
        rect.width /= 3f;
        rect.x += rect.width * 3;
        if(names2Valid[element])
        {
            if (GUI.Button(rect, "Editor"))
            {
                AssetDatabase.OpenAsset(
                    AssetDatabase.LoadAssetAtPath<Object>($"Assets/{_content.cs_path}/{element}.cs"));
            }

            rect.x += rect.width;
            if (GUI.Button(rect, "Remove"))
            {
                if (EditorUtility.DisplayDialog("警告", $"是否删除{element}事件", "确定", "取消"))
                {
                    File.Delete($"{Application.dataPath}/{_content.cs_path}/{element}.cs.meta");
                    File.Delete($"{Application.dataPath}/{_content.cs_path}/{element}.cs");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
        else
        {
            rect.width *= 2;
            Color color = GUI.color;
            GUI.color=Color.yellow/1.5f;
            GUI.Box(rect,"不在指定路径!");
            GUI.color = color;
        }
    }

    private void SelectEventItem(ReorderableList list)
    {
        
    }
}