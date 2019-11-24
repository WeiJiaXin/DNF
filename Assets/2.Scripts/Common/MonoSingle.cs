using UnityEngine;

public class MonoSingle<T> : MonoBehaviour where T : MonoSingle<T>
{
    private static T m_ins;

    public static T Ins
    {
        get
        {
            if (m_ins == null)
                m_ins = FindObjectOfType<T>();
            if (m_ins == null)
                m_ins = new GameObject(typeof(T).Name, typeof(T)).GetComponent<T>();
            return m_ins;
        }
    }
}