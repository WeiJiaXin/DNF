// Copyright (C) 2017-2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Utility class for deleting the PlayerPrefs from within the editor.
/// </summary>
public class DeletePlayerPrefs
{
    [MenuItem("Tools/Delete AllStorage", false, Int32.MaxValue)]
    public static void DeleteAllStorage()
    {
        PlayerPrefs.DeleteAll();
        KVStorage.DeleteAll();
        Directory.Delete(Application.persistentDataPath,true);
    }
    
    [MenuItem("Tools/Menu/Run By Launcher")]
    static void RunByLauncher()
    {
        EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/01.Scenes/Launcher.unity");
    }
    [MenuItem("Tools/Menu/Run By Launcher",true)]
    static bool RunIsByLauncher()
    {
        return EditorSceneManager.playModeStartScene == null;
    }
    [MenuItem("Tools/Menu/Run By Current")]
    static void RunByCurrent()
    {
        EditorSceneManager.playModeStartScene = null;
    }
    [MenuItem("Tools/Menu/Run By Current",true)]
    static bool RunIsByCurrent()
    {
        return EditorSceneManager.playModeStartScene != null;
    }
}