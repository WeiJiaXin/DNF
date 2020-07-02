using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CsvHandle;
using Lowy.Bind;
using UnityEngine;

namespace Lowy.Table
{
    public class Table<T> where T : ITable, new()
    {
        public static Table<T> Ins => Binder.GetInstance<Table<T>>();
        //
        protected string path;
        protected string name;
        //
        public List<T> Data { get; set; }

        protected Dictionary<object, T> key2T;
        protected Dictionary<object, List<T>> key2TList;

        public Table() : this(TableSetter.Path)
        {
        }

        public Table(string path):this(path,typeof(T).Name)
        {
        }

        public Table(string path, string name)
        {
            this.path = path;
            this.name = name;
            LoadData();
            if (Data != null)
                RefreshList();
        }

        protected virtual void LoadData()
        {
            if (string.IsNullOrEmpty(path))
                return;
            var content = Resources.Load<TextAsset>(path + "/" + name);
            if (content == null)
                return;
            Data = CsvHelper.ReadContent<T>(content.text);
        }

        protected virtual void RefreshList()
        {
            //
            key2T = new Dictionary<object, T>(Data.Count);
            key2TList = new Dictionary<object, List<T>>();
            foreach (var data in Data)
            {
                if (key2TList.ContainsKey(data.Key()))
                {
                    key2TList[data.Key()].Add(data);
                    continue;
                }

                if (key2T.ContainsKey(data.Key()))
                {
                    key2TList.Add(data.Key(), new List<T>());
                    key2TList[data.Key()].Add(key2T[data.Key()]);
                    key2TList[data.Key()].Add(data);
                    key2T.Remove(data.Key());
                    continue;
                }

                key2T.Add(data.Key(), data);
            }
        }

        public virtual T GetModel(object k)
        {
            if (key2T.ContainsKey(k))
                return key2T[k];
            if (key2T.Count>0)
            {
                var enumerator = key2T.Values.GetEnumerator();
                enumerator.MoveNext();
                if (enumerator.Current is ITableLerp<T> t)
                    return t.Lerp(key2T, k);
            }
            return default;
        }

        public virtual List<T> GetAllModel(object k)
        {
            if (key2TList.ContainsKey(k))
                return key2TList[k];
            if (key2T.ContainsKey(k))
                return new List<T> {key2T[k]};
            return null;
        }

        public virtual List<T> GetAllModel(Func<T,bool> filter=null)
        {
            List<T> list = new List<T>();
            foreach (var kv in key2T)
            {
                if (filter == null || filter.Invoke(kv.Value))
                    list.Add(kv.Value);
            }

            foreach (var kv in key2TList)
            {
                foreach (var table in kv.Value)
                {
                    if (filter == null || filter.Invoke(table))
                        list.Add(table);
                }
            }

            return list;
        }
    }
}