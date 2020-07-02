using UnityEngine;

public struct Pool<T> where T : MonoBehaviour
{
    private Pool _pool;

    public Pool(Pool p)
    {
        _pool = p;
    }

    public T Pop(Transform parent)
    {
        return _pool.PopObj(parent).GetComponent<T>();
    }

    public void Push(T t)
    {
        _pool.PushObj(t.gameObject);
    }
}