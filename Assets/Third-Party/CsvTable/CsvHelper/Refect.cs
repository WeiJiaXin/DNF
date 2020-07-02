using System;
using System.Collections.Generic;
using System.Reflection;

namespace CsvHandle {
    /// <summary>
    ///     反射泛型中的属性,只反射拥有setter的属性
    /// </summary>
    internal class Refect {
        /// <summary>
        ///     反射到的属性
        /// </summary>
        public List<PropertyInfo> Property;
        /// <summary>
        ///     拥有<see cref="WriteValueAttribute" />的属性标签
        /// </summary>
        public List<WriteValueAttribute> PropertyName;
        /// <summary>
        /// 最后是否有多参
        /// </summary>
        public bool hasParams=false;

        public Refect(Type type) {
            var ps        = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var infos     = new List<PropertyInfo>();
            var infoNames = new List<WriteValueAttribute>();
            foreach (var item in ps) {
                if (item.GetCustomAttribute<NonWriteValueAttribute>() != null)
                    continue;
                if (!item.CanWrite) {
                    continue;
                }

                infos.Add(item);
                var att = item.GetCustomAttribute<WriteValueAttribute>();
                if (att != null) {
                    att.m_property = item;
                    infoNames.Add(att);
                }

                if (item.GetCustomAttribute<ParamsAttribute>() != null)
                    hasParams = true;
            }

            Property     = infos;
            PropertyName = infoNames;
        }
    }
}