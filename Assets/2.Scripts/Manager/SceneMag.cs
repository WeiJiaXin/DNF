using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lowy.Scene
{
    /// <summary>
    /// 场景管理,TODO 对外状态,回调,多场景
    /// </summary>
    public class SceneMag : MonoSingle<SceneMag>
    {
        //当前场景信息
        private SceneInfo current;

        //正在加载的场景,LoadScene后将异步等待此场景加载
        private SceneInfo loadingScene;

        //正在预加载的场景,不作为正在预加载的标识,Scene可能在异步实例化中
        private Dictionary<SceneInfo, ResourceRequest> preLoadingScene;

        //下一次调用预加载的索引,调用PreloadScene()时,使用此索引检索current中的nextScenes.
        private int preLoadIndex;

        #region UnityFunc

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            preLoadingScene = new Dictionary<SceneInfo, ResourceRequest>();
            //修改线程优先级
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            //初始化地图
            InitMap();
        }

        private void Update()
        {
            List<SceneInfo> removeScenes = new List<SceneInfo>(preLoadingScene.Count);
            //资源加载成功后开携程继续执行异步实例化操作,并在removeScenes删除
            foreach (var kv in preLoadingScene)
            {
                if (kv.Value.isDone)
                {
                    StartCoroutine(InstantiateAsync(kv.Key, kv.Value.asset as GameObject));
                    removeScenes.Add(kv.Key);
                }
            }

            foreach (var scene in removeScenes)
            {
                preLoadingScene.Remove(scene);
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
            SceneInfo info = SceneInfo.GetOrCreate(Scenes.City_Home);
            info.nextScenes.Add(SceneInfo.GetOrCreate(Scenes.City_SilverVillage));
            //
            info = SceneInfo.GetOrCreate(Scenes.City_SilverVillage);
            info.nextScenes.Add(SceneInfo.GetOrCreate(Scenes.City_Home));
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
                    if (SceneInfo.AllScene.ContainsKey(info.nextScenes[i].name))
                        nextDeath.AddLast(info.nextScenes[i]);
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
            SceneManager.LoadScene(Scenes.MAIN, LoadSceneMode.Single);
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
                var info = current.nextScenes[preLoadIndex];
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
            if (info.preLoad)
                return;
            //
            info.preLoad = true;
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
                UnloadCurrentScene();
                current = null;
                return LoadScene(name);
            }

            var info = SceneInfo.GetOrCreate(scene);
            if (info.preLoad)
            {
                if (info.prefab != null)
                {
                    //加载场景
                    info.prefab.SetActive(true);
                    UnloadCurrentScene();
                    current = info;
                    preLoadIndex = 0;
                    return false;
                }
            }
            else
            {
                //预加载,此时需要外部等待
                PreLoadScene(info);
            }

            loadingScene = info;
            return true;
        }

        /// <summary>
        /// 异步实例化
        /// </summary>
        /// <param name="info">实例化的场景</param>
        /// <param name="prefab">Unity预制物</param>
        /// <returns></returns>
        private IEnumerator InstantiateAsync(SceneInfo info, GameObject prefab)
        {
            var g = new GameObject(prefab.name);
            g.SetActive(false);
            for (int i = 0; i < prefab.transform.childCount; i++)
            {
                Instantiate(prefab.transform.GetChild(i), g.transform);
                yield return 0;
            }

            info.prefab = g;

            if (loadingScene != null && loadingScene.name == info.name)
            {
                LoadScene(loadingScene.name);
                loadingScene = null;
            }
        }

        #endregion

        #region UnloadScene

        /// <summary>
        /// 卸载当前场景
        /// </summary>
        public void UnloadCurrentScene()
        {
            UnloadScene(current);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="info"></param>
        public void UnloadScene(SceneInfo info)
        {
            if (info == null)
                return;
            info.Dispose();
            StartCoroutine(DestroyAsync(info));
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
        private IEnumerator DestroyAsync(SceneInfo info)
        {
            info.preLoad = false;
            var o = info.prefab;
            info.prefab = null;
            List<GameObject> childs = new List<GameObject>(o.transform.childCount);
            for (int i = 0; i < o.transform.childCount; i++)
            {
                childs.Add(o.transform.GetChild(i).gameObject);
            }

            //避免脚本中update有空指针
            o.SetActive(false);
            foreach (var t in childs)
            {
                Destroy(t);
                yield return 0;
            }

            Destroy(o);
        }

        #endregion
    }
}