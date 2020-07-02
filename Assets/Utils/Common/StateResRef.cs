using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateResRef : MonoBehaviour
{
    [System.Serializable]
    public struct Data
    {
        public string name;
        public UnityEngine.Object obj;
    }

    //
    private Dictionary<string, Object> m_name2Obj;

    private Dictionary<string, Object> name2Obj
    {
        get
        {
            if (m_name2Obj == null)
            {
                m_name2Obj = new Dictionary<string, Object>();
                foreach (var data in datas)
                {
                    m_name2Obj[data.name] = data.obj;
                }
            }

            return m_name2Obj;
        }
    }

    public string StateTag => stateTag;
    [SerializeField] private string stateTag;

    [SerializeField] public List<Data> datas;

    public T Get<T>(string n) where T : Object
    {
        if (name2Obj.ContainsKey(n))
        {
            if (name2Obj[n] is T t)
                return t;
            Debug.LogError($"StateResRef Get 类型错误，想要{typeof(T).Name},拿到的却是{name2Obj[n].GetType().Name}");
            return null;
        }

        return null;
    }
}