using System;

namespace CsvHandle
{
    /// <inheritdoc />
    [CustomConvert(true,
        typeof(int),typeof(uint),
        typeof(short),typeof(ushort),
        typeof(long),typeof(ulong),
        typeof(float),typeof(double),typeof(decimal),
        typeof(string),typeof(char),
        typeof(byte),typeof(sbyte),
        typeof(bool),typeof(Enum))]
    public class BaseTypeConvert:ICustomConvert
    {
        /// <inheritdoc />
        public object Parse(Type infoPropertyType, string csvStr)
        {
            if (string.IsNullOrEmpty(csvStr))
                return default;
            if (infoPropertyType == typeof(string))
                return csvStr;
            if (infoPropertyType == typeof(int))
                return string.IsNullOrEmpty(csvStr) ? 0 : int.Parse(csvStr);
            if (infoPropertyType == typeof(uint))
                return string.IsNullOrEmpty(csvStr) ? 0 : uint.Parse(csvStr);
            if (infoPropertyType == typeof(short))
                return string.IsNullOrEmpty(csvStr) ? 0 : short.Parse(csvStr);
            if (infoPropertyType == typeof(ushort))
                return string.IsNullOrEmpty(csvStr) ? 0 : ushort.Parse(csvStr);
            if (infoPropertyType == typeof(long))
                return string.IsNullOrEmpty(csvStr) ? 0 : long.Parse(csvStr);
            if (infoPropertyType == typeof(ulong))
                return string.IsNullOrEmpty(csvStr) ? 0 : ulong.Parse(csvStr);
            if (infoPropertyType == typeof(float))
                return string.IsNullOrEmpty(csvStr) ? 0 : float.Parse(csvStr);
            if (infoPropertyType == typeof(double))
                return string.IsNullOrEmpty(csvStr) ? 0 : double.Parse(csvStr);
            if (infoPropertyType == typeof(decimal))
                return string.IsNullOrEmpty(csvStr) ? 0 : decimal.Parse(csvStr);
            if (infoPropertyType == typeof(char))
                return string.IsNullOrEmpty(csvStr) ? 0 : char.Parse(csvStr);
            if (infoPropertyType == typeof(byte))
                return string.IsNullOrEmpty(csvStr) ? 0 : byte.Parse(csvStr);
            if (infoPropertyType == typeof(sbyte))
                return string.IsNullOrEmpty(csvStr) ? 0 : sbyte.Parse(csvStr);
            if (infoPropertyType == typeof(bool))
                return !string.IsNullOrEmpty(csvStr) && bool.Parse(csvStr);
            if (infoPropertyType.IsEnum)
                return Enum.Parse(infoPropertyType,
                    string.IsNullOrEmpty(csvStr) ? Enum.GetNames(infoPropertyType)[0] : csvStr, true);
            return default;
        }
    }
}