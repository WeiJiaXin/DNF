using System;
using System.Collections.Generic;
using System.Linq;
using PreviewLabs;

public static class KVStorage
{
    static KVStorage()
    {
#if UNITY_EDITOR
        PlayerPrefs.EnableEncryption(false);
#else
        PlayerPrefs.EnableEncryption(true);
#endif
    }

    public static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteFile();
    }

    public static void Flush()
    {
        PlayerPrefs.Flush();
    }

    public static void SetIntList(string key, List<int> numbers)
    {
        if (numbers == null || numbers.Count == 0)
        {
            PlayerPrefs.DeleteKey(key);
            return;
        }

        var value = string.Join(",", numbers);
        PlayerPrefs.SetString(key, value);
    }

    public static List<int> GetIntList(string key)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            return null;
        }

        var value = PlayerPrefs.GetString(key);
        return value.Split(',').Select(int.Parse).ToList();
    }

    public static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetBool(key, value);
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        return PlayerPrefs.GetBool(key, defaultValue);
    }

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public static int GetInt(string key)
    {
        return PlayerPrefs.GetInt(key);
    }

    public static int GetInt(string key, int defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    public static float GetFloat(string key)
    {
        return PlayerPrefs.GetFloat(key);
    }

    public static float GetFloat(string key, float defaultValue)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public static string GetString(string key, string defaultValue = null)
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public static DateTime GetDateTime(string key, DateTime? defaultValue = null)
    {
        DateTime dateTime;
        if (HasKey(key))
        {
            var content = PlayerPrefs.GetString(key);
            if (!DateTime.TryParse(content, out dateTime))
            {
                dateTime = defaultValue ?? DateTime.Now;
            }
        }
        else
        {
            dateTime = defaultValue ?? DateTime.Now;
        }

        return dateTime;
    }

    public static void SetDateTime(string key, DateTime? dateTime)
    {
        if (null == dateTime)
        {
            PlayerPrefs.DeleteKey(key);
        }
        else
        {
            PlayerPrefs.SetString(key, dateTime.Value.ToString());
        }
    }
}