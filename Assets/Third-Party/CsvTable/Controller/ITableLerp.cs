using System.Collections.Generic;

namespace Lowy.Table
{
    public interface ITableLerp<T>
    {
        T Lerp(Dictionary<object, T> o_dic, object o_key);
    }
}