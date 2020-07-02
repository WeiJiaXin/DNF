using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lowy.Bind
{
    public class Binder
    {
        private static Dictionary<Type, Binding> _dic;
        private static IReflection reflection;

        private const int LOCK_MAX_COUNT = 100;
        private static int lockCount = 0;

        static Binder()
        {
            reflection = new Reflection();
            _dic = new Dictionary<Type, Binding>();
        }

        public static Binding Bind<K>()
        {
            return Bind(typeof(K));
        }

        public static Binding Bind(Type k)
        {
            if (_dic.ContainsKey(k))
                return _dic[k];
            var binding = new Binding(k);
            _dic.Add(k, binding);
            return _dic[k];
        }

        public static void UnBind<K>(string name = null)
        {
            UnBind(typeof(K),name);
        }
        public static void UnBind(Type k,string name = null)
        {
            if(GetBind(k)==null)
                return;
            GetBind(k).UnBindAll();
            _dic.Remove(k);
        }

        public static Binding GetBind<K>()
        {
            return GetBind(typeof(K));
        }
        public static Binding GetBind(Type k)
        {
            if (_dic.ContainsKey(k))
                return _dic[k];
            return null;
        }

        public static void InjectObj(object obj)
        {
            var fields = reflection.GetFieldByAttribute<InjectAttribute>(obj);
            foreach (var field in fields)
            {
                string name = field.GetCustomAttribute<InjectAttribute>().Name;
                field.SetValue(obj, GetInstance(field.FieldType, name));
            }
        }

        public static K GetInstance<K>(Enum name, params object[] pars) => GetInstance<K>(name.ToString(), pars);

        public static K GetInstance<K>(string name = null, params object[] pars) =>
            (K) GetInstance(typeof(K), name, pars);

        public static object GetInstance(Type k, string name = null, params object[] pars)
        {
            if (lockCount >= LOCK_MAX_COUNT)
            {
                lockCount = 0;
                throw new InjectException("注入递归异常，递归出现循环。超出递归最大次数：" + LOCK_MAX_COUNT);
            }

            lockCount++;
            object o = null;
            if (_dic.ContainsKey(k))
                o = _dic[k].GetInstance(name, pars);
            lockCount = 0;
            return o;
        }

        public static void ReflectionAll()
        {
            foreach (var value in _dic.Values)
            {
                value.ReflectionAll();
            }
        }
    }
}