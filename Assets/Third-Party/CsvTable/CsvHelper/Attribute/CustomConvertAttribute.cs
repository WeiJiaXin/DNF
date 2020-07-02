using System;

namespace CsvHandle
{
    /// <summary>
    /// 自定义转换 - csvString to a object
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Class)] 
    public class CustomConvertAttribute : Attribute
    {
        /// <summary>
        /// 是否可以被覆盖替换,内部预制的转换器为可被覆盖
        /// </summary>
        public bool CanReplace { get; }

        /// <summary>
        /// 转换器支持的类型
        /// </summary>
        public Type[] Types { get; }
        /// <summary>
        /// 自定义转换 - csvString to a object
        /// </summary>
        /// <param name="types"></param>
        public CustomConvertAttribute(params Type[] types)
        {
            Types = types;
        }

        /// <summary>
        /// 自定义转换 - csvString to a object
        /// </summary>
        /// <param name="canReplace">当有多个转换器时,此转换器是否可以被覆盖</param>
        /// <param name="types">支持的类型</param>
        public CustomConvertAttribute(bool canReplace, params Type[] types):this(types)
        {
            CanReplace = canReplace;
        }
    }
}