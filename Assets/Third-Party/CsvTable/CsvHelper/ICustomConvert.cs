using System;

namespace CsvHandle
{
    /// <summary>
    /// 自定义转换接口 - csvString to a object
    /// </summary>
    public interface ICustomConvert
    {
        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="infoPropertyType"></param>
        /// <param name="csvStr"></param>
        /// <returns></returns>
        object Parse(Type infoPropertyType, string csvStr);
    }
}