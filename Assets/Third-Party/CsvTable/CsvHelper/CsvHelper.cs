using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;


namespace CsvHandle
{
    /// <summary>
    ///     CSV文件读取类
    /// </summary>
    /// <summary>
    ///     <para> </para>
    ///     可以写入的只能是属性
    ///     <para>私有属性将直接忽略,数据存在的话,会报错</para>
    ///     加了WriteValueAttribute标签的, 将会用设置的name
    ///     <para>加了WriteValueAttribute标签的, 必须有setter, 否则将会略过并报错</para>
    ///     加了NonWriteValueAttribute标签的, 将不会被写入
    ///     <para>没有加标签的, 如果有setter, 将会写入, 如果数据不存在, 会报错</para>
    ///     没有加标签的, 如果没有setter, 将会略过
    ///     <para>数组分割使用'|','|'需要转义'\|'</para>
    ///     任何字符串都会处理'\?'的转义 ('\?'->'?')
    ///     <para> </para>
    ///     建议场景加载或静止动画时多次调用
    ///     <para>CsvHelper.RefectType[T]();</para>
    ///     或单次调用
    ///     <para>CsvHelper.RefectType(class1, class2, class3,...);</para>
    ///     来提前做好反射工作
    ///     <para> </para>
    ///     var res = CsvHelper.ReadFile[MyClass](path, Encoding.GetEncoding("gb2312"));
    /// </summary>
    public sealed class CsvHelper
    {
        //反射信息的缓存字典
        private static Dictionary<Type, Refect> m_cacheRefects;
        private static Dictionary<Type, ICustomConvert> m_customConvert;

        private static char noteChar = '#';

        //如果读取过文件,这里存放最后一次的文件开始注解
        private static List<string> m_lastFileTitleNote;

        /// <summary>
        ///     返回最后一次的文件开始注解(包含注解字符,如'#')
        /// </summary>
        public static List<List<string>> LastFileTitleNote
        {
            get
            {
                if (m_lastFileTitleNote == null || m_lastFileTitleNote.Count <= 0)
                    return null;
                return ReadValue(m_lastFileTitleNote);
            }
        }

        /// <summary>
        ///     静态构造
        /// </summary>
        static CsvHelper()
        {
            m_cacheRefects = new Dictionary<Type, Refect>();
            //
            var ts = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ICustomConvert))))
                .ToArray();
            m_customConvert = new Dictionary<Type, ICustomConvert>(ts.Length);
            foreach (var t in ts)
            {
                var ic = Activator.CreateInstance(t,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, null, null) as ICustomConvert;
                var att = t.GetCustomAttribute<CustomConvertAttribute>();
                if (att != null)
                {
                    foreach (var type in att.Types)
                    {
                        if (m_customConvert.ContainsKey(type))
                        {
                            //如果不可被覆盖,则直接使用.可被替换,就忽略这个
                            if (!att.CanReplace)
                                m_customConvert[type] = ic;
                        }
                        else
                            m_customConvert.Add(type, ic);
                    }
                }
            }
        }

        /// <summary>
        ///     读取文件
        /// </summary>
        /// <typeparam name="T">文件中存放的类型</typeparam>
        /// <param name="fullName">文件名(包含路径)</param>
        /// <param name="encoding">编码方式,中文可能是gb2312</param>
        /// <param name="note">文件开始注解,注解的开头是什么字符(比如把第一行作为中文注解,第二行才是属性名,则第一行是#notes)</param>
        /// <returns>读取的对象集合</returns>
        public static List<T> ReadFile<T>(string fullName, Encoding encoding, char note = '#') where T : new()
        {
            return ReadContent<T>(File.ReadAllText(fullName, encoding), note);
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <typeparam name="T">文件中存放的类型</typeparam>
        /// <param name="content">文件内容</param>
        /// <param name="note">文件开始注解,注解的开头是什么字符(比如把第一行作为中文注解,第二行才是属性名,则第一行是#notes)</param>
        /// <returns>读取的对象集合</returns>
        public static List<T> ReadContent<T>(string content, char note = '#') where T : new()
        {
            noteChar = note;
            var typeT = typeof(T);
            //如果没有就反射并缓存反射信息 
            if (!m_cacheRefects.ContainsKey(typeT))
                m_cacheRefects.Add(typeT, new Refect(typeT));
            //
            var lines = new List<string>(content.Split(new char[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries));
            m_lastFileTitleNote = new List<string>();
            while (true)
                if (lines[0][0] == note || (lines[0][0] == '"' && lines[0][1] == note))
                {
                    m_lastFileTitleNote.Add(lines[0]);
                    lines.RemoveAt(0);
                }
                else
                {
                    break;
                }

            //
            var names = ReadTitle(lines[0]);
            lines.RemoveAt(0);
            var values = ReadValue(lines);
            //
            return Serialize<T>(names, values);
        }

        /// <summary>
        ///     提前反射需要的类型并缓存
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        public static void RefectType<T>()
        {
            RefectType(typeof(T));
        }

        /// <summary>
        ///     提前反射需要的类型并缓存
        /// </summary>
        /// <param name="typeT">类型</param>
        public static void RefectType(params Type[] typeT)
        {
            foreach (var item in typeT)
                RefectType(item);
        }

        /// <summary>
        ///     提前反射需要的类型并缓存
        /// </summary>
        /// <param name="typeT">类型</param>
        private static void RefectType(Type typeT)
        {
            //如果没有就反射并缓存反射信息
            if (!m_cacheRefects.ContainsKey(typeT))
                m_cacheRefects.Add(typeT, new Refect(typeT));
        }

        //读取头部,即属性名
        private static List<string> ReadTitle(string titleStr)
        {
            var title = new List<string>(titleStr.Trim().Split(','));
            //多参属性,可能会导致title最后又好多,,,,,
            for (int i = title.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(title[i]))
                    title.RemoveAt(i);
                else
                    break;
            }
            return title;
        }

        //读取数据部分,此时不解析
        private static List<List<string>> ReadValue(List<string> lines)
        {
            var resList = new List<List<string>>();
            foreach (var line in lines)
            {
                //如果是注释则跳过
                if (line.StartsWith($"{noteChar}") || line.StartsWith($"\"{noteChar}"))
                    continue;
                var valueList = new List<string>();
                var datas = line.Split(',');
                var queue = new Queue<string>();
                foreach (var data in datas)
                    if (queue.Count <= 0)
                    {
                        if (data.StartsWith("\""))
                        {
                            if (Regex.Matches(data, "\"").Count % 2 == 0)
                                valueList.Add(DelESC(data.Substring(1, data.Length - 2), '\"'));
                            else
                                queue.Enqueue(data);
                        }
                        else
                        {
                            valueList.Add(data);
                        }
                    }
                    else
                    {
                        if (Regex.Matches(data, "\"").Count % 2 == 1)
                        {
                            queue.Enqueue(data);
                            valueList.Add(DelESC(GetStringByQueue(queue, ','), '\"'));
                            queue.Clear();
                        }
                        else
                        {
                            queue.Enqueue(data);
                        }
                    }

                resList.Add(valueList);
            }

            return resList;
        }

        //根据属性和Type序列化这些数据
        private static List<T> Serialize<T>(List<string> names, List<List<string>> values) where T : new()
        {
            var list = new List<T>();
            var type = typeof(T);
            if (!m_cacheRefects.ContainsKey(type))
                throw new Exception("反射属性失败,字典中不存在");
            var refect = m_cacheRefects[type];
            var pros = new List<PropertyInfo>();
            for (var i = 0; i < names.Count; i++)
            {
                PropertyInfo info = null;
                var proName = refect.PropertyName.Find(p => p.m_name == names[i]);
                if (proName == null)
                    info = refect.Property.Find(p => p.Name == names[i]);
                else
                    info = proName.m_property;
                pros.Add(info);
            }

            for (var i = 0; i < values.Count; i++)
            {
                var t = new T();
                int maxCount = pros.Count;
                if (refect.hasParams)
                    maxCount--;
                for (var j = 0; j < maxCount; j++)
                    SetValue(pros[j], t, values[i][j]);
                if (refect.hasParams)
                {
                    int lenght = values[i].Count;
                    for (int j = values[i].Count - 1; j >= 0; j--)
                    {
                        if (string.IsNullOrEmpty(values[i][j]))
                            --lenght;
                        else
                            break;
                    }
                    Array array = SetArrayValue(pros[maxCount], t, lenght - maxCount);
                    int index = 0;
                    for (int j = maxCount; j < lenght; j++, index++)
                    {
                        SetParamsValue(pros[maxCount], t, array, values[i][j], index);
                    }
                }

                list.Add(t);
            }

            return list;
        }

        //从队列中拼接字符串并删去前后的字符
        private static string GetStringByQueue(Queue<string> queue, char esc, bool delSide = true)
        {
            var str = "";
            if (queue.Count <= 0)
                return "";
            str += queue.Dequeue();
            for (; queue.Count > 0;) str += esc + queue.Dequeue();
            if (delSide)
                str = str.Substring(1, str.Length - 2);
            return str;
        }

        //删去字符串中的转义
        private static string DelESC(string str, char esc)
        {
            var count = 0;
            var cs = new char[str.Length];
            for (var i = 0; i < str.Length; i++, count++)
            {
                if (str[i] == esc) i++;
                cs[count] = str[i];
            }

            return new string(cs, 0, count);
        }

        /// <summary>
        ///     将值中的数据赋值到obj中
        /// </summary>
        /// <param name="info">字段属性</param>
        /// <param name="obj">需要被赋值的对象</param>
        /// <param name="val">字段的值</param>
        private static void SetValue(PropertyInfo info, object obj, string val)
        {
            if (info == null)
                //class中不存在这个属性
                return;
            if (info.PropertyType.IsArray)
            {
                //处理数组
                var vals = SpiltArray(val);
                var eleType = info.PropertyType.GetElementType();
                if (eleType == null)
                    return;
                var array = Array.CreateInstance(eleType, vals.Count);
                for (int i = 0; i < vals.Count; i++)
                {
                    if (m_customConvert.ContainsKey(eleType))
                    {
                        object v = null;
                        try
                        {
                            v = m_customConvert[eleType].Parse(eleType, vals[i].Trim());
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(eleType + "\n" + vals[i].Trim() + "\n" + e.ToString());
                        }

                        array.SetValue(v, i);
                    }
                }

                return;
            }

            Type t = info.PropertyType.IsEnum ? typeof(Enum) : info.PropertyType;
            if (m_customConvert.ContainsKey(t))
            {
                object v = null;
                try
                {
                    //处理普通类型 todo 抛出详细异常
                    v = m_customConvert[t].Parse(info.PropertyType, DelESC(val, '\\').Trim());
                }
                catch (InvalidCastException e)
                {
                    Debug.LogError(info.PropertyType + "\n" + DelESC(val, '\\').Trim() + "\n" + e.ToString());
                }

                info.SetValue(obj, v);
            }
        }

        /// <summary>
        ///     将值中的数据添加到obj的多参集合中
        /// </summary>
        /// <param name="info">字段属性</param>
        /// <param name="obj">需要被赋值的对象</param>
        /// <param name="array"></param>
        /// <param name="val">字段的值</param>
        /// <param name="index"></param>
        private static void SetParamsValue(PropertyInfo info, object obj, Array array, string val, int index)
        {
            var eleType = info.PropertyType.GetElementType();
            if (m_customConvert.ContainsKey(eleType))
            {
                object v = null;
                try
                {
                    v = m_customConvert[eleType].Parse(eleType, val.Trim());
                }
                catch (Exception e)
                {
                    Debug.LogError(eleType + "\n" + val.Trim() + "\n" + e.ToString());
                }

                array.SetValue(v, index);
            }
            else
                Debug.LogError(eleType + "\n" + val.Trim() + "\n" + "类型转换不存在转换器");
        }

        /// <summary>
        /// 创建一个数组,并赋值
        /// </summary>
        /// <param name="info">数组类型</param>
        /// <param name="obj">对象</param>
        /// <param name="length">数组长度</param>
        /// <returns>创建的数组实例</returns>
        private static Array SetArrayValue(PropertyInfo info, object obj, int length)
        {
            var eleType = info.PropertyType.GetElementType();
            if (eleType == null)
                return null;
            Array array = Array.CreateInstance(eleType, length);
            info.SetValue(obj, array);
            return array;
        }

        //裁切数组,会进行转义
        private static List<string> SpiltArray(string val)
        {
            var strs = val.Split(',');
            List<string> list = new List<string>();
            Queue<string> queue = new Queue<string>();
            for (int i = 0; i < strs.Length; i++)
            {
                if (Regex.Matches(strs[i], "\\\\").Count % 2 != 0)
                    queue.Enqueue(strs[i].Substring(0, strs[i].Length - 1));
                else
                {
                    if (queue.Count <= 0)
                    {
                        list.Add(DelESC(strs[i], '\\'));
                    }
                    else
                    {
                        queue.Enqueue(strs[i]);
                        list.Add(DelESC(GetStringByQueue(queue, ',', false), '\\'));
                        queue.Clear();
                    }
                }
            }

            return list;
        }
    }
}