using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class Singleton<T> where T : Singleton<T>
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (null == instance)
            {
                instance = Activator.CreateInstance<T>();
            }

            return instance;
        }
    }
}

public class MonoSingleton<T> : MonoForDebug where T : MonoSingleton<T>
{
    protected static T m_ins;
    public static T Instance
    {
        get
        {
            if (!m_ins)
            {
                m_ins = Object.FindObjectOfType<T>();
            }

            return m_ins;
        }
    }

    protected virtual void Awake()
    {
        m_ins = this as T;
    }
}