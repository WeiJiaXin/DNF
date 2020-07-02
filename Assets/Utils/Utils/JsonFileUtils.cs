using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using Newtonsoft.Json;

/// <summary>
/// Miscellaneous file utilities.
/// </summary>
public static class JsonFileUtils
{
    public static void SaveJsonFile<T>(string path, T data) where T : class
    {
        var str = JsonConvert.SerializeObject(data);
        var f = new StreamWriter(path);
        f.WriteLine(str);
        f.Close();
    }

    public static T LoadObjectToFile<T>(string path) where T : class
    {
        if (!new FileInfo(path).Exists)
        {
            return null;
        }

        //Read the text file
        StreamReader reader = new StreamReader(path);
        string text = reader.ReadToEnd();
        reader.Close();
        return JsonConvert.DeserializeObject<T>(text);
    }
}