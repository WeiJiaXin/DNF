using System;
using System.Reflection;

namespace Lowy.Bind
{
    internal interface IReflection
    {
        object GetInstance(Type t, params object[] args);
        FieldInfo[] GetFieldByAttribute<T>(object obj) where T : Attribute;
        ReflectionData ReflectionType(Type type);
    }
}