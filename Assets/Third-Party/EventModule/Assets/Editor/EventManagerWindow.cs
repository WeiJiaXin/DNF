using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Lowy.Event;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public partial class EventManagerWindow : EditorWindow
{
    private EventWindowContent _content;
    private EventSettingContent _setting;

    [MenuItem("Window/Lowy/EventManage %e")]
    public static void CreateView()
    {
        //创建窗口
        EventManagerWindow window = GetWindow();
        window.Show();
    }

    public static EventManagerWindow GetWindow()
    {
        Rect wr = new Rect(0, 0, 500, 550);
        return (EventManagerWindow) EditorWindow.GetWindowWithRect(typeof(EventManagerWindow), wr, false,
            "Event Manage Window");
    }

    private void OnEnable()
    {
        _content = EventUtility.GetContent();
        _setting = EventUtility.GetSetting();
    }

    private void OnDisable()
    {
        EventUtility.SaveContent(_content);
        EventUtility.SaveSetting(_setting);
    }

    private void OnGUI()
    {
        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        {
            _content.tab = GUILayout.Toggle(_content.tab == 0, "首选项", EditorStyles.toolbarButton) ? 0 : _content.tab;
            _content.tab = GUILayout.Toggle(_content.tab == 1, "管理Event", EditorStyles.toolbarButton)
                ? 1
                : _content.tab;
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        switch (_content.tab)
        {
            case 0:
                SettingGUI();
                break;
            case 1:
                ManagerEventGUI();
                break;
        }
    }

    private void SettingGUI()
    {
        //------------------------------
        //Editor Setting
        //------------------------------
        EditorGUILayout.LabelField("Editor Setting:", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.BeginHorizontal();
            _content.cs_path = EditorGUILayout.TextField("File Path：", _content.cs_path);
            if (GUILayout.Button("Select Event File Path", GUILayout.MaxWidth(150)))
            {
                string path = SelectPath();
                if (!string.IsNullOrEmpty(path) && path.Contains(Application.dataPath + "/"))
                {
                    _content.cs_path = path.Replace(Application.dataPath + "/", "");
                }
                else if (path != null && path.Contains(Application.dataPath + "/"))
                {
                    EditorUtility.DisplayDialog("EventManager",
                        $"'{path}' Path invalid, path need has '{Application.dataPath}/'", "ok");
                }
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        //------------------------------
        //Runtime Setting
        //------------------------------
        EditorGUILayout.LabelField("Runtime Setting:", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            _setting.dispatchExceptionInterrupt =
                EditorGUILayout.Toggle("调度发生异常时是否中断", _setting.dispatchExceptionInterrupt);
        }
        EditorGUILayout.EndVertical();
    }

    private void ManagerEventGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            DrawEventList();
        }
        EditorGUILayout.EndHorizontal();
        //
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("请尽量使用 <Eve> 作为事件后缀");
            _content.eventName = EditorGUILayout.TextField("Event Name:", _content.eventName);
            if (GUILayout.Button("Add Event", GUILayout.Height(30)))
            {
                EventUtility.SaveContent(_content);
                if (CanCreate(_content))
                {
                    CreateEventFile(_content);
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "其中部分 Event 已存在", "ok");
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private bool CanCreate(EventWindowContent content)
    {
        var names = content.eventName.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
        foreach (var n in names)
        {
            if (File.Exists($"{Application.dataPath}/{content.cs_path}/{n}.cs"))
                return false;
        }

        return true;
    }

    private string SelectPath()
    {
        return EditorUtility.SaveFolderPanel("Select path", Application.dataPath, "Assets");
    }

    private void CreateEventFile(EventWindowContent content)
    {
        string path = $"{Application.dataPath}/{content.cs_path}";
        CreatePath(path);
        var names = content.eventName.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
        foreach (var n in names)
        {
            string text = $"public class {n} : Lowy.Event.IEvent {{ }}";
            //
            File.WriteAllText($"{path}/{n}.cs",
                text);
        }
    }

    private void CreatePath(string s)
    {
        if (!Directory.Exists(s))
            Directory.CreateDirectory(s);
    }
}