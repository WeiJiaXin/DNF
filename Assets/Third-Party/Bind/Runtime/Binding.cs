using System;
using System.Collections.Generic;

namespace Lowy.Bind
{
    public class Binding
    {
        private const string NONE_NAME = "NONE_NAME";

        private Type m_TypeK;
        private IReflection reflection;
        private Dictionary<string, object> _dicName2Obj;
        private Dictionary<string, bool> _dicName2Single;
        private bool _allSingle;

        public Binding(Type k)
        {
            m_TypeK = k;
            reflection = new Reflection();
            _dicName2Obj = new Dictionary<string, object>();
            _dicName2Single = new Dictionary<string, bool>();
        }

        public Binding To<V>()
        {
            var v = typeof(V);
            if (_dicName2Obj.ContainsKey(NONE_NAME))
                _dicName2Obj[NONE_NAME] = v;
            else
                _dicName2Obj.Add(NONE_NAME, v);
            return this;
        }

        public Binding To<V>(V val)
        {
            if (_dicName2Obj.ContainsKey(NONE_NAME))
                _dicName2Obj[NONE_NAME] = val;
            else
                _dicName2Obj.Add(NONE_NAME, val);
            return this;
        }

        public Binding ToName(Enum name)
        {
            return ToName(name.ToString());
        }

        public Binding ToName(string name)
        {
            if (_dicName2Obj.ContainsKey(NONE_NAME))
            {
                var val = _dicName2Obj[NONE_NAME];
                _dicName2Obj.Remove(NONE_NAME);
                _dicName2Obj.Add(name, val);
            }
            else
            {
                _dicName2Obj.Add(name, m_TypeK);
            }

            return this;
        }

        public Binding ToSingle(string name = NONE_NAME)
        {
            if (_allSingle)
                return this;
            if (_dicName2Single.ContainsKey(name))
                _dicName2Single[name] = true;
            else
                _dicName2Single.Add(name,true);
            return this;
        }

        public Binding ToSingleForAll()
        {
            _allSingle = true;
            return this;
        }

        public void UnBind(Enum name)
        {
            UnBind(name.ToString());
        }
        public void UnBind(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = NONE_NAME;
            if (_dicName2Obj.ContainsKey(name))
            {
                _dicName2Obj.Remove(name);
            }
            if (_dicName2Single.ContainsKey(name))
            {
                _dicName2Single.Remove(name);
            }
        }

        public void UnBindAll()
        {
            _dicName2Obj.Clear();
            _dicName2Single.Clear();
        }

        public object GetInstance(Enum name, params object[] pars) => GetInstance(name.ToString(), pars);

        public object GetInstance(string name, params object[] pars)
        {
            if (string.IsNullOrEmpty(name))
                name = NONE_NAME;
            if (_dicName2Obj.ContainsKey(name))
            {
                object o = _dicName2Obj[name];
                if (o is Type type)
                {
                    object k = reflection.GetInstance(type, pars);
                    if (_allSingle||
                        (_dicName2Single.ContainsKey(name) && _dicName2Single[name]))
                        _dicName2Obj[name] = k;
                    return k;
                }

                return _dicName2Obj[name];
            }

            //返回null
            return default;
        }

        public void ReflectionAll()
        {
            foreach (var value in _dicName2Obj.Values)
            {
                if (value is Type type)
                    reflection.ReflectionType(type);
            }
        }
    }
}