using System;
using System.Reflection;

namespace CsvHandle {
    /// <summary>
    ///     表示需要写入value,并使用自定义name
    /// </summary>
    public class WriteValueAttribute : Attribute {
        /// <summary>
        /// 自定的name
        /// </summary>
        public string m_name;
        /// <summary>
        /// 属性信息,在反射时填充
        /// </summary>
        public PropertyInfo m_property;

        /// <summary>
        ///     初始化 <see cref="T:System.Attribute" /> 类的新实例。
        /// </summary>
        /// <param name="name">传入在文件中的字段名,避免使用'#'开头</param>
        public WriteValueAttribute(string name) {
            m_name = name;
        }
    }
}