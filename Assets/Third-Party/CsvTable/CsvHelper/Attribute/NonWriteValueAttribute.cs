using System;

namespace CsvHandle {
    /// <summary>
    ///     表示不需要写入value,如果没有这个属性标签,将会以属性名作为name
    /// </summary>
    public class NonWriteValueAttribute : Attribute { }
}