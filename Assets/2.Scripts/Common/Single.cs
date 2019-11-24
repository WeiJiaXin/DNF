using UnityEngine;

public class Single<T> where T : Single<T>, new()
{
    private T m_ins;

    public T Ins => m_ins ?? (m_ins = new T());
}