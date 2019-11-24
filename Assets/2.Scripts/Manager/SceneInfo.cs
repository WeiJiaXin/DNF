using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lowy.Scene
{
    /// <summary>
    /// 场景信息
    /// </summary>
    public class SceneInfo
    {
        #region Static Data

        /// <summary>
        /// 场景图,里面为作为静态信息
        /// </summary>
        private static Dictionary<string, SceneInfo> Map;

        /// <summary>
        /// 当前预加载或加载后的场景,不可缓存此字典
        /// <para>使用时需要判断<see cref="nextScenes"/>是否存在<see cref="AllScene"/>中,不存在则表示是一条无效的边</para>
        /// </summary>
        public static Dictionary<string, SceneInfo> AllScene = new Dictionary<string, SceneInfo>();

        /// <summary>
        /// 获取或创建一个场景,与Map中同名不同对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">生成场景图时,没有此场景的数据,或是没有调用<see cref="SceneInfo.ToMap"/></exception>
        public static SceneInfo GetOrCreate(string name)
        {
            if (!Map.ContainsKey(name))
                throw new ArgumentNullException(message: "Map中不存在此场景信息", null);
            if (!AllScene.ContainsKey(name))
                AllScene[name] = new SceneInfo(name);
            return AllScene[name];
        }

        /// <summary>
        /// 将当前的<see cref="AllScene"/>创建为Map,必须在初始化后调用
        /// </summary>
        public static void ToMap()
        {
            Map = AllScene;
            AllScene = new Dictionary<string, SceneInfo>();
        }

        #endregion

        #region Object Data

        /// <summary>
        /// 场景名
        /// </summary>
        public string name;

        /// <summary>
        /// 最终加载出的预制物,Instantiate后的物体,不要主动操作
        /// </summary>
        public GameObject prefab;

        /// <summary>
        /// 是否已经预加载,为<see cref="true"/>只是标记为预加载,如果<see cref="prefab"/>为null,则正在加载
        /// </summary>
        public bool preLoad;

        /// <summary>
        /// 连接的下一场景
        /// </summary>
        public readonly List<SceneInfo> nextScenes;

        /// <summary>
        /// 根据名字构造
        /// </summary>
        /// <param name="name"></param>
        public SceneInfo(string name)
        {
            this.name = name;
            nextScenes = new List<SceneInfo>();
            HandleEdge();
        }

        /// <summary>
        /// 从Map中读取连接的场景
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void HandleEdge()
        {
            if (Map.ContainsKey(name))
                throw new ArgumentNullException(message: "Map中不存在此场景信息", null);
            SceneInfo mapInfo = Map[name];
            nextScenes.AddRange(mapInfo.nextScenes);
        }

        /// <summary>
        /// 从<see cref="AllScene"/>删除,无需处理连接的场景信息
        /// </summary>
        public void Dispose()
        {
            if (AllScene.ContainsKey(name))
                AllScene.Remove(name);
        }

        #endregion
    }
}