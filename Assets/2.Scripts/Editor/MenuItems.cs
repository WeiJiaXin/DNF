using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MenuItems : MonoBehaviour
{
    [MenuItem("Tools/Menu/ClearProgressBar")]
    public static void ClearProgressBar()
    {
        EditorUtility.ClearProgressBar();
    }
    [MenuItem("Tools/Menu/文件分类")]
    public static void FileToDir()
    {
        string root = Application.dataPath + "/../ImagePacks2/";
        string[] files = Directory.GetFiles(root);
        int i = 0;
        foreach (var name in files)
        {
            EditorUtility.DisplayProgressBar("进度",$"分类文件: {i+1}/{files.Length}",i/(float)files.Length);
            CopyToDir(name);
            i++;
        }
        EditorUtility.ClearProgressBar();
    }

    private static void CopyToDir(string name)
    {
        name = Path.GetFileName(name);
        string path = Application.dataPath + "/../ImagePacks2Output/";
        string[] ns = name.Split('_');
        int index = 0;
        while (index+1!=ns.Length)
        {
            path += ns[index] + "/";
            index++;
        }

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        if(File.Exists(path+ns[index]))
            return;
        File.Copy(Application.dataPath + "/../ImagePacks2/"+name,path+ns[index]);
    }
}
