using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lowy.Scene
{
    public delegate void Loaded(SceneInfo info, bool isAdditive);
    /// <summary>
    /// 场景管理,TODO 对外状态,回调,多场景
    /// </summary>
    public class SceneMag : MonoSingleton<SceneMag>
    {
        public static bool async;
        //当前场景信息
        public SceneInfo current;
        public List<SceneInfo> additiveScenes;

        //场景加载结束回调
        public event Loaded onLoaded;

        //正在加载的场景,LoadScene后将异步等待此场景加载
        private Dictionary<SceneInfo, bool> loadingScene2IsAdditive;
        private List<SceneInfo> needUnloadScene;

        //正在预加载的场景,不作为正在预加载的标识,Scene可能在异步实例化中
        private Dictionary<SceneInfo, ResourceRequest> preLoadingScene;

        //下一次调用预加载的索引,调用PreloadScene()时,使用此索引检索current中的nextScenes.
        private int preLoadIndex;

        #region UnityFunc

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            additiveScenes = new List<SceneInfo>();
            loadingScene2IsAdditive = new Dictionary<SceneInfo, bool>();
            needUnloadScene = new List<SceneInfo>();
            preLoadingScene = new Dictionary<SceneInfo, ResourceRequest>();
            //修改线程优先级
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            //初始化地图
            InitMap();
        }
        

        private void Update()
        {
            //资源加载成功后开携程继续执行异步实例化操作
            var keys = preLoadingScene.Keys.ToList();
            foreach (var key in keys)
            {
                var value = preLoadingScene[key];
                if (value.isDone)
                {
                    key.prefab = value.asset as GameObject;
                    if (async)
                        StartCoroutine(InstantiateAsync(key));
                    else
                        Instantiate(key);
                    preLoadingScene.Remove(key);
                }
            }
        }

        #endregion

        #region Implement

        /// <summary>
        /// 初始化地图信息和结构
        /// <para>至少所有场景都要存在</para>
        /// </summary>
        private void InitMap()
        {
            SceneInfo info = SceneInfo.GetOrCreate(Scenes.Home);
            info.nextScenes.Add(SceneInfo.GetOrCreate(Scenes.Game).name);
            info.nextScenes.Add(SceneInfo.GetOrCreate(Scenes.CrossLevelGame).name);
            info.nextScenes.Add(SceneInfo.GetOrCreate(Scenes.Home).name);
            //
            info = SceneInfo.GetOrCreate(Scenes.Game);
            info.nextScenes.Add(SceneInfo.GetOrCreate(Scenes.Home).name);
            info.nextScenes.Add(SceneInfo.GetOrCreate(Scenes.Game).name);
            info.nextScenes.Add(SceneInfo.GetOrCreate(Scenes.CrossLevelGame).name);
            //
            info = SceneInfo.GetOrCreate(Scenes.CrossLevelGame);
            info.nextScenes.Add(SceneInfo.GetOrCreate(Scenes.Home).name);
            info.nextScenes.Add(SceneInfo.GetOrCreate(Scenes.CrossLevelGame).name);
            //
            SceneInfo.ToMap();
            /*
             * TODO 可转到配置表
             * name,nextNames
             * A,"B,C"
             * B,A
             * C,A
             *
             *    A
             *   ↙  ↘
             *  B xx C
             */
        }

        /// <summary>
        /// 广度优先遍历(分层)
        /// </summary>
        /// <param name="link">初始链表</param>
        /// <param name="callback">节点回调操作</param>
        /// <param name="d">传入节点所在深度</param>
        private void BFS(LinkedList<SceneInfo> link, Action<SceneInfo, int> callback, int d = 0)
        {
            d++;
            LinkedList<SceneInfo> nextDeath = new LinkedList<SceneInfo>();
            while (link.Count > 0)
            {
                var info = link.First.Value;
                link.RemoveFirst();
                callback?.Invoke(info, d);

                for (int i = 0; i < info.nextScenes.Count; i++)
                {
                    if (SceneInfo.AllScene.ContainsKey(info.nextScenes[i]))
                        nextDeath.AddLast(SceneInfo.GetOrCreate(info.nextScenes[i]));
                }
            }

            while (nextDeath.Count > 0)
            {
                var first = nextDeath.First.Value;
                nextDeath.RemoveFirst();
                if (!link.Contains(first))
                    link.AddLast(first);
            }

            if (link.Count <= 0)
                return;
            BFS(link, callback, d);
        }

        #endregion

        #region LoadScene

        /// <summary>
        /// 加载主场景
        /// </summary>
        public void LoadUnityMainScene()
        {
            SceneManager.LoadScene(Scenes.MAINUnityScene, LoadSceneMode.Single);
        }

        /// <summary>
        /// 预加载一次连接的场景
        /// </summary>
        public void PreLoadScene()
        {
            if (string.IsNullOrEmpty(current.name))
                return;
            if (current.nextScenes.Count <= preLoadIndex)
                return;
            for (; preLoadIndex < current.nextScenes.Count;)
            {
                var info = SceneInfo.GetOrCreate(current.nextScenes[preLoadIndex]);
                if (info.preLoad)
                {
                    preLoadIndex++;
                    continue;
                }

                PreLoadScene(info);
                //设置下一次的索引
                preLoadIndex++;
                break;
            }
        }

        /// <summary>
        /// 预加载指定的场景
        /// </summary>
        /// <param name="info">场景信息</param>
        public void PreLoadScene(SceneInfo info)
        {
            if (info.preLoad && info.obj != null)
                return;
            //
            info.preLoad = true;
            if (!preLoadingScene.ContainsKey(info))
                preLoadingScene.Add(info, Resources.LoadAsync(info.name));
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="scene">场景名</param>
        /// <returns>是否需要等待,可使用---监听</returns>
        public bool LoadScene(string scene)
        {
            if (current != null && current.name == scene)
            {
                //重新加载场景
                UnloadCurrentScene(true);
                current = null;
                return LoadScene(scene);
            }

            var info = SceneInfo.GetOrCreate(scene);
            if (info.preLoad)
            {
                if (info.obj != null)
                {
                    //加载场景
                    UnloadCurrentScene(false);
                    current = info;
                    preLoadIndex = 0;
                    info.obj.SetActive(true);
                    onLoaded?.Invoke(info,false);
                    return false;
                }
                else
                {
                    if (!async)
                    {
                        loadingScene2IsAdditive.Add(info, false);
                        Instantiate(info);
                        onLoaded?.Invoke(info,false);
                        return false;
                    }
                }
            }

            //预加载,此时需要外部等待
            PreLoadScene(info);

            loadingScene2IsAdditive.Add(info, false);
            return true;
        }

        /// <summary>
        /// 增加场景
        /// </summary>
        /// <param name="scene">场景名</param>
        /// <returns>是否需要等待,可使用---监听</returns>
        public bool AddScene(string scene)
        {
            if (current == null)
            {
                throw new Exception("没有主场景");
            }

            var info = SceneInfo.GetOrCreate(scene);
            if (info.preLoad)
            {
                if (info.obj != null)
                {
                    //加载场景
                    info.obj.SetActive(true);
                    additiveScenes.Add(info);
                    onLoaded?.Invoke(info,true);
                    return false;
                }
                else
                {
                    if (!async)
                    {
                        loadingScene2IsAdditive.Add(info, true);
                        Instantiate(info);
                        onLoaded?.Invoke(info,true);
                        return false;
                    }
                }
            }

            //预加载,此时需要外部等待
            PreLoadScene(info);

            loadingScene2IsAdditive.Add(info, true);
            return true;
        }

        /// <summary>
        /// 异步实例化
        /// </summary>
        /// <param name="info">实例化的场景</param>
        /// <returns></returns>
        private IEnumerator InstantiateAsync(SceneInfo info)
        {
            if(info.obj==null)
            {
                GameObject prefab = info.prefab;
                var g = new GameObject(prefab.name);
                g.SetActive(false);
                g.transform.position = prefab.transform.position;
                g.transform.rotation = prefab.transform.rotation;
                g.transform.localScale = prefab.transform.localScale;

                for (int i = 0; i < prefab.transform.childCount; i++)
                {
                    Instantiate(prefab.transform.GetChild(i), g.transform).name = prefab.transform.GetChild(i).name;
                    yield return 0;
                }

                info.obj = g;
            }

            foreach (var kv in loadingScene2IsAdditive)
            {
                if (kv.Key.name == info.name)
                {
                    if (kv.Value)
                    {
                        AddScene(kv.Key.name);
                    }
                    else
                        LoadScene(kv.Key.name);

                    loadingScene2IsAdditive.Remove(kv.Key);
                    break;
                }
            }
        }

        private void Instantiate(SceneInfo info)
        {
            if(info.obj==null)
            {
                info.prefab.gameObject.SetActive(false);
                var obj = Instantiate(info.prefab);
                info.obj = obj;
                info.prefab.gameObject.SetActive(true);
            }

            foreach (var kv in loadingScene2IsAdditive)
            {
                if (kv.Key.name == info.name)
                {
                    if (kv.Value)
                    {
                        AddScene(kv.Key.name);
                    }
                    else
                        LoadScene(kv.Key.name);

                    loadingScene2IsAdditive.Remove(kv.Key);
                    break;
                }
            }
        }

        #endregion

        #region UnloadScene

        /// <summary>
        /// 卸载当前场景
        /// <param name="preUnload">需要预缓存卸载场景</param>
        /// </summary>
        public void UnloadCurrentScene(bool preUnload)
        {
            if (preUnload)
            {
                if(current!=null)
                {
                    needUnloadScene.Add(current.Clone());
                    current.obj = null;
                    foreach (var info in additiveScenes)
                    {
                        needUnloadScene.Add(info.Clone());
                        info.obj = null;
                    }
                }

                additiveScenes.Clear();
                return;
            }
            else
            {
                if (current != null)
                {
                    needUnloadScene.Add(current);
                    foreach (var info in additiveScenes)
                    {
                        needUnloadScene.Add(info);
                    }

                    additiveScenes.Clear();
                }
            }

            UnloadScene(needUnloadScene.ToArray());
            needUnloadScene.Clear();
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="info"></param>
        public void UnloadScene(params SceneInfo[] infos)
        {
            if (infos == null || infos.Length <= 0)
                return;
            foreach (var info in infos)
            {
                info?.Dispose();
            }

            StartCoroutine(DestroyAsync(infos));
        }

        /// <summary>
        /// 卸载远处的场景
        /// </summary>
        /// <param name="d">卸载的最小深度</param>
        public void UnloadAwayScene(int d = 3)
        {
            List<SceneInfo> infos = new List<SceneInfo>();
            var linkedList = new LinkedList<SceneInfo>();
            linkedList.AddFirst(current);
            BFS(linkedList, UnloadScene);
            foreach (var info in infos)
            {
                this.UnloadScene(info);
            }

            //
            void UnloadScene(SceneInfo info, int cd)
            {
                if (cd >= d)
                {
                    infos.Add(info);
                }
            }
        }

        /// <summary>
        /// 异步销毁一个场景
        /// </summary>
        /// <param name="info">场景信息</param>
        /// <returns></returns>
        private IEnumerator DestroyAsync(params SceneInfo[] infos)
        {
            int index = 0;
            foreach (var info in infos)
            {
                if (info == null)
                    continue;
                var o = info.obj;
                if (o != null)
                    //避免脚本中update有空指针
                    o.SetActive(false);
            }
            while (infos.Length > index)
            {
                var info = infos[index];
                index++;
                if (info == null)
                    continue;
                var o = info.obj;
                info.obj = null;
                if (o != null)
                {
                    List<GameObject> childs = new List<GameObject>(o.transform.childCount);
                    for (int i = 0; i < o.transform.childCount; i++)
                    {
                        childs.Add(o.transform.GetChild(i).gameObject);
                    }

                    foreach (var t in childs)
                    {
                        Destroy(t);
                        yield return 0;
                    }

                    Destroy(o);
                }
            }
        }

        #endregion
    }
}